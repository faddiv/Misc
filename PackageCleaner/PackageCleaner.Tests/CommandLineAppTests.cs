using System.CommandLine;

namespace PackageCleaner.Tests;

/// <summary>Runs the whole tool on a temporary project folder and packages folder.</summary>
public class CommandLineAppTests
{
    private static (int ExitCode, string Output) Run(string input, bool isInteractive, params string[] args)
    {
        var output = new StringWriter();
        var exitCode = CommandLineApp.Build(output, new StringReader(input), isInteractive)
            .Parse(args)
            .Invoke(new InvocationConfiguration { Output = output, Error = output });
        return (exitCode, output.ToString());
    }

    private static (int ExitCode, string Output) RunYes(TempFolder temp, params string[] args) =>
        Run("", false, [.. args, temp.Combine("repo"), "--packages-path", temp.Combine("packages"), "--yes"]);

    private static (string Used, string Old, string Unused) CreateCache(TempFolder temp)
    {
        temp.RestoredProject("repo/App/App.csproj", "Used/2.0.0");
        return (temp.CachedPackage("used", "2.0.0"), temp.CachedPackage("used", "1.0.0"), temp.CachedPackage("unused", "1.0.0"));
    }

    [Test]
    public async Task Removes_old_versions_and_unused_packages()
    {
        using var temp = new TempFolder();
        var (used, old, unused) = CreateCache(temp);

        var (exitCode, output) = RunYes(temp);

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(Directory.Exists(used)).IsTrue();
        await Assert.That(Directory.Exists(old)).IsFalse();
        await Assert.That(Directory.Exists(unused)).IsFalse();
        await Assert.That(Directory.Exists(temp.Combine("packages/unused"))).IsFalse();
        await Assert.That(output).Contains("Deleted 2 items");
    }

    [Test]
    public async Task Nuget_command_runs_the_cleaner()
    {
        using var temp = new TempFolder();
        var (used, old, _) = CreateCache(temp);

        var (exitCode, _) = RunYes(temp, "nuget");

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(Directory.Exists(used)).IsTrue();
        await Assert.That(Directory.Exists(old)).IsFalse();
    }

    [Test]
    public async Task Several_folders_use_the_union_of_their_projects()
    {
        using var temp = new TempFolder();
        temp.RestoredProject("first/App/App.csproj", "One/1.0.0");
        temp.RestoredProject("second/App/App.csproj", "Two/1.0.0");
        var one = temp.CachedPackage("one", "1.0.0");
        var two = temp.CachedPackage("two", "1.0.0");
        var three = temp.CachedPackage("three", "1.0.0");

        Run("", false, temp.Combine("first"), temp.Combine("second"), "--packages-path", temp.Combine("packages"), "-y");

        await Assert.That(Directory.Exists(one)).IsTrue();
        await Assert.That(Directory.Exists(two)).IsTrue();
        await Assert.That(Directory.Exists(three)).IsFalse();
    }

    [Test]
    public async Task Dry_run_changes_nothing()
    {
        using var temp = new TempFolder();
        var (_, old, unused) = CreateCache(temp);

        var packages = temp.Combine("packages");
        var (exitCode, output) = Run("", false, "nuget", temp.Combine("repo"), "--packages-path", packages, "--dry-run");

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(Directory.Exists(old)).IsTrue();
        await Assert.That(Directory.Exists(unused)).IsTrue();
        await Assert.That(output).Contains("used 1.0.0");
        await Assert.That(output).Contains("Total: 2 items");
    }

    [Test]
    [Arguments("y", false)]
    [Arguments("YES", false)]
    [Arguments("n", true)]
    [Arguments("", true)]
    public async Task Prompt_decides_whether_to_delete(string answer, bool kept)
    {
        using var temp = new TempFolder();
        var (_, old, _) = CreateCache(temp);

        var (exitCode, _) = Run(answer, true, temp.Combine("repo"), "--packages-path", temp.Combine("packages"));

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(Directory.Exists(old)).IsEqualTo(kept);
    }

    [Test]
    public async Task Non_interactive_input_without_yes_deletes_nothing()
    {
        using var temp = new TempFolder();
        var (_, old, _) = CreateCache(temp);

        var (exitCode, output) = Run("y", false, temp.Combine("repo"), "--packages-path", temp.Combine("packages"));

        await Assert.That(exitCode).IsEqualTo(1);
        await Assert.That(Directory.Exists(old)).IsTrue();
        await Assert.That(output).Contains("--yes");
    }

    [Test]
    public async Task No_restored_project_deletes_nothing()
    {
        using var temp = new TempFolder();
        temp.File("repo/App/App.csproj");
        var package = temp.CachedPackage("some", "1.0.0");

        var (exitCode, output) = RunYes(temp);

        await Assert.That(exitCode).IsEqualTo(1);
        await Assert.That(Directory.Exists(package)).IsTrue();
        await Assert.That(output).Contains("No restored project");
    }

    [Test]
    public async Task Corrupt_assets_file_deletes_nothing()
    {
        using var temp = new TempFolder();
        var (_, old, _) = CreateCache(temp);
        temp.File("repo/App/obj/project.assets.json", "{ not json");

        var (exitCode, _) = RunYes(temp);

        await Assert.That(exitCode).IsEqualTo(1);
        await Assert.That(Directory.Exists(old)).IsTrue();
    }

    [Test]
    public async Task Ignored_projects_are_listed()
    {
        using var temp = new TempFolder();
        CreateCache(temp);
        var ignored = temp.File("repo/Other/Other.csproj");

        var (_, output) = RunYes(temp);

        await Assert.That(output).Contains($"Ignored, not restored: {ignored}");
    }

    [Test]
    public async Task Removes_leftovers_even_when_the_version_is_used()
    {
        using var temp = new TempFolder();
        var (used, _, _) = CreateCache(temp);
        temp.File("packages/used/.deleting-2.0.0/lib/a.dll");

        RunYes(temp);

        await Assert.That(Directory.Exists(temp.Combine("packages/used/.deleting-2.0.0"))).IsFalse();
        await Assert.That(Directory.Exists(used)).IsTrue();
    }

    [Test]
    public async Task Nothing_to_clean_succeeds()
    {
        using var temp = new TempFolder();
        temp.RestoredProject("repo/App/App.csproj", "Used/2.0.0");
        temp.CachedPackage("used", "2.0.0");

        var (exitCode, output) = RunYes(temp);

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(output).Contains("Nothing to clean.");
    }

    [Test]
    [Arguments("")]
    [Arguments("nuget")]
    public async Task Missing_folder_is_an_error(string commandLine)
    {
        var (exitCode, _) = Run("", false, commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        await Assert.That(exitCode).IsNotEqualTo(0);
    }

    [Test]
    public async Task Nonexistent_folder_is_an_error()
    {
        using var temp = new TempFolder();

        var (exitCode, _) = Run("", false, temp.Combine("missing"));

        await Assert.That(exitCode).IsNotEqualTo(0);
    }
}
