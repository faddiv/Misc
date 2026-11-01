using PackageCleaner.NuGet;

namespace PackageCleaner.Tests;

public class UsedPackagesCollectorTests
{
    private static CollectedPackages Collect(TempFolder temp, TextWriter? log = null) =>
        UsedPackagesCollector.Collect(ProjectScanner.Scan([temp.Path], TextWriter.Null), log ?? TextWriter.Null);

    [Test]
    public async Task Reads_package_libraries_and_ignores_project_ones()
    {
        using var temp = new TempFolder();
        temp.File("App/App.csproj", "<Project />");
        temp.File("App/obj/project.assets.json", """
            {
              "libraries": {
                "Some.Package/1.0.0": { "type": "package", "path": "some.package/1.0.0" },
                "Other.Project/1.0.0": { "type": "project", "path": "../Other/Other.csproj" }
              }
            }
            """);

        var collected = Collect(temp);

        await Assert.That(collected.Used.IsUsed("some.package", "1.0.0")).IsTrue();
        await Assert.That(collected.Used.IsUsed("other.project", "1.0.0")).IsFalse();
    }

    [Test]
    public async Task Reads_download_dependencies()
    {
        using var temp = new TempFolder();
        temp.File("App/App.csproj", "<Project />");
        temp.File("App/obj/project.assets.json", """
            {
              "libraries": {},
              "project": {
                "frameworks": {
                  "net8.0": {
                    "downloadDependencies": [ { "name": "Microsoft.NETCore.App.Ref", "version": "[8.0.22, 8.0.22]" } ]
                  }
                }
              }
            }
            """);

        var used = Collect(temp).Used;

        await Assert.That(used.IsUsed("microsoft.netcore.app.ref", "8.0.22")).IsTrue();
        await Assert.That(used.IsUsed("microsoft.netcore.app.ref", "8.0.16")).IsFalse();
    }

    [Test]
    public async Task Project_without_assets_file_is_ignored_with_everything_it_names()
    {
        using var temp = new TempFolder();
        var project = temp.File("App/App.csproj", """
            <Project Sdk="Some.Sdk/1.0.0">
              <ItemGroup><PackageReference Include="Some.Package" Version="1.0.0" /></ItemGroup>
            </Project>
            """);

        var collected = Collect(temp);

        await Assert.That(collected.IgnoredProjects).IsEquivalentTo([project]);
        await Assert.That(collected.RestoredProjects).IsEmpty();
        await Assert.That(collected.Used.IsUsed("some.package", "1.0.0")).IsFalse();
        await Assert.That(collected.Used.IsUsed("some.sdk", "1.0.0")).IsFalse();
    }

    [Test]
    [Arguments("{ not json")]
    [Arguments("{}")]
    [Arguments("""{ "libraries": { "A/1.0.0": { "type": "package" } } }""")]
    public async Task Unreadable_assets_file_stops_the_run(string content)
    {
        using var temp = new TempFolder();
        temp.File("App/App.csproj", "<Project />");
        temp.File("App/obj/project.assets.json", content);

        await Assert.That(() => Collect(temp)).Throws<CleanerException>();
    }

    [Test]
    public async Task Malformed_project_file_only_warns()
    {
        using var temp = new TempFolder();
        var project = temp.File("App/App.csproj", "<Project");
        temp.File("App/obj/project.assets.json", """{ "libraries": {} }""");
        var log = new StringWriter();

        var collected = Collect(temp, log);

        await Assert.That(collected.RestoredProjects).IsEquivalentTo([project]);
        await Assert.That(log.ToString()).Contains(project);
    }

    [Test]
    public async Task Reads_every_form_of_sdk_reference()
    {
        using var temp = new TempFolder();
        temp.File("App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk;First.Sdk/1.0.0;Second.Sdk/2.0.0">
              <Sdk Name="Third.Sdk" Version="3.0.0" />
              <Sdk Name="Unversioned.Sdk" />
              <Import Project="Sdk.props" Sdk="Fourth.Sdk/4.0.0" />
            </Project>
            """);
        temp.File("App/obj/project.assets.json", """{ "libraries": {} }""");

        var used = Collect(temp).Used;

        await Assert.That(used.IsUsed("first.sdk", "1.0.0")).IsTrue();
        await Assert.That(used.IsUsed("second.sdk", "2.0.0")).IsTrue();
        await Assert.That(used.IsUsed("third.sdk", "3.0.0")).IsTrue();
        await Assert.That(used.IsUsed("fourth.sdk", "4.0.0")).IsTrue();
        await Assert.That(used.IsUsed("unversioned.sdk", "1.0.0")).IsFalse();
        await Assert.That(used.IsUsed("microsoft.net.sdk", "1.0.0")).IsFalse();
    }

    [Test]
    public async Task Reads_package_pins_even_with_no_restored_user()
    {
        using var temp = new TempFolder();
        temp.File("Directory.Packages.props", """
            <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
              <ItemGroup>
                <PackageVersion Include="Pinned" Version="1.0.0" />
                <PackageVersion Include="Floating" Version="2.*" />
                <PackageVersion Include="Variable" Version="$(SomeVersion)" />
                <GlobalPackageReference Include="Global" Version="3.0.0" />
              </ItemGroup>
            </Project>
            """);

        var used = Collect(temp).Used;

        await Assert.That(used.IsUsed("pinned", "1.0.0")).IsTrue();
        await Assert.That(used.IsUsed("pinned", "1.0.1")).IsFalse();
        await Assert.That(used.IsUsed("floating", "2.5.0")).IsTrue();
        await Assert.That(used.IsUsed("floating", "3.0.0")).IsFalse();
        await Assert.That(used.IsUsed("variable", "7.7.7")).IsTrue();
        await Assert.That(used.IsUsed("global", "3.0.0")).IsTrue();
    }

    [Test]
    public async Task Reads_msbuild_sdks_from_global_json_with_comments()
    {
        using var temp = new TempFolder();
        temp.File("global.json", """
            {
              // Comments are allowed here.
              "msbuild-sdks": { "Some.Sdk": "1.0.0", },
            }
            """);

        var used = Collect(temp).Used;

        await Assert.That(used.IsUsed("some.sdk", "1.0.0")).IsTrue();
    }

    [Test]
    public async Task Reads_tool_manifest()
    {
        using var temp = new TempFolder();
        temp.File(".config/dotnet-tools.json", """
            { "version": 1, "tools": { "dotnet-ef": { "version": "8.0.1", "commands": [ "dotnet-ef" ] } } }
            """);

        var used = Collect(temp).Used;

        await Assert.That(used.IsUsed("dotnet-ef", "8.0.1")).IsTrue();
        await Assert.That(used.IsUsed("dotnet-ef", "8.0.0")).IsFalse();
    }

    [Test]
    public async Task Malformed_manifest_only_warns()
    {
        using var temp = new TempFolder();
        var manifest = temp.File(".config/dotnet-tools.json", "{ not json");
        var log = new StringWriter();

        Collect(temp, log);

        await Assert.That(log.ToString()).Contains(manifest);
    }
}
