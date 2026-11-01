using System.Diagnostics;
using PackageCleaner.NuGet;
using TUnit.Core.Enums;

namespace PackageCleaner.Tests;

public class ProjectScannerTests
{
    [Test]
    [Arguments(".git")]
    [Arguments("node_modules")]
    [Arguments("bin")]
    [Arguments("obj")]
    [Arguments(".vs")]
    [Arguments(".idea")]
    [Arguments("BIN")]
    public async Task Skips_well_known_folders(string name)
    {
        using var temp = new TempFolder();
        temp.File($"{name}/Hidden.csproj");

        var result = ProjectScanner.Scan([temp.Path], TextWriter.Null);

        await Assert.That(result.ProjectFiles).IsEmpty();
    }

    [Test]
    [Arguments("App.csproj")]
    [Arguments("App.fsproj")]
    [Arguments("App.vbproj")]
    [Arguments("App.CSPROJ")]
    public async Task Finds_project_and_stops_recursing(string fileName)
    {
        using var temp = new TempFolder();
        var project = temp.File($"src/{fileName}");
        temp.File("src/nested/Nested.csproj");

        var result = ProjectScanner.Scan([temp.Path], TextWriter.Null);

        await Assert.That(result.ProjectFiles).IsEquivalentTo([project]);
    }

    [Test]
    public async Task Keeps_recursing_past_a_folder_with_only_package_props()
    {
        using var temp = new TempFolder();
        var props = temp.File("repo/Directory.Packages.props");
        var project = temp.File("repo/src/App/App.csproj");

        var result = ProjectScanner.Scan([temp.Path], TextWriter.Null);

        await Assert.That(result.PackagePropsFiles).IsEquivalentTo([props]);
        await Assert.That(result.ProjectFiles).IsEquivalentTo([project]);
    }

    [Test]
    public async Task Finds_tool_manifests_and_global_json_in_a_project_folder()
    {
        using var temp = new TempFolder();
        temp.File("App/App.csproj");
        var config = temp.File("App/.config/dotnet-tools.json");
        var plain = temp.File("App/dotnet-tools.json");
        var globalJson = temp.File("App/global.json");

        var result = ProjectScanner.Scan([temp.Path], TextWriter.Null);

        await Assert.That(result.ToolManifests).IsEquivalentTo([config, plain]);
        await Assert.That(result.GlobalJsonFiles).IsEquivalentTo([globalJson]);
    }

    [Test]
    public async Task Finds_tool_manifest_in_a_folder_with_no_project()
    {
        using var temp = new TempFolder();
        var manifest = temp.File(".config/dotnet-tools.json");

        var result = ProjectScanner.Scan([temp.Path], TextWriter.Null);

        await Assert.That(result.ToolManifests).IsEquivalentTo([manifest]);
    }

    [Test]
    public async Task Reads_overlapping_folders_once()
    {
        using var temp = new TempFolder();
        var project = temp.File("repo/App/App.csproj");

        var result = ProjectScanner.Scan([temp.Path, temp.Combine("repo")], TextWriter.Null);

        await Assert.That(result.ProjectFiles).IsEquivalentTo([project]);
    }

    [Test]
    public async Task A_given_folder_is_scanned_even_with_a_skipped_name()
    {
        using var temp = new TempFolder();
        var project = temp.File("bin/App/App.csproj");

        var result = ProjectScanner.Scan([temp.Combine("bin")], TextWriter.Null);

        await Assert.That(result.ProjectFiles).IsEquivalentTo([project]);
    }

    [Test]
    [RunOn(OS.Windows)]
    public async Task Does_not_follow_junctions()
    {
        using var temp = new TempFolder();
        temp.File("target/App.csproj");
        var scanned = temp.Folder("scanned");
        var junction = Path.Combine(scanned, "link");
        var mklink = Process.Start(new ProcessStartInfo("cmd", ["/c", "mklink", "/J", junction, temp.Combine("target")])
        {
            RedirectStandardOutput = true,
        })!;
        await mklink.WaitForExitAsync();

        var result = ProjectScanner.Scan([scanned], TextWriter.Null);
        var created = Directory.Exists(junction);
        Directory.Delete(junction);

        await Assert.That(created).IsTrue();
        await Assert.That(result.ProjectFiles).IsEmpty();
    }
}
