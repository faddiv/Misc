using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace PackageCleaner.NuGet;

public sealed record CollectedPackages(
    UsedPackages Used,
    IReadOnlyList<string> RestoredProjects,
    IReadOnlyList<string> IgnoredProjects);

/// <summary>Reads the files found by <see cref="ProjectScanner"/> into a <see cref="UsedPackages"/>.</summary>
public static class UsedPackagesCollector
{
    // global.json and dotnet-tools.json are edited by hand, and the SDK accepts comments in them.
    private static readonly JsonDocumentOptions _handWrittenJson = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    public static CollectedPackages Collect(ScanResult scan, TextWriter log)
    {
        var used = new UsedPackages();
        var restored = new List<string>();
        var ignored = new List<string>();

        foreach (var project in scan.ProjectFiles)
        {
            var assetsFile = Path.Combine(Path.GetDirectoryName(project)!, "obj", "project.assets.json");
            if (!File.Exists(assetsFile))
            {
                ignored.Add(project);
                continue;
            }

            ReadAssets(assetsFile, used);
            ReadProjectSdks(project, used, log);
            restored.Add(project);
        }

        foreach (var props in scan.PackagePropsFiles)
        {
            ReadPackageVersions(props, used, log);
        }

        foreach (var globalJson in scan.GlobalJsonFiles)
        {
            ReadGlobalJson(globalJson, used, log);
        }

        foreach (var manifest in scan.ToolManifests)
        {
            ReadToolManifest(manifest, used, log);
        }

        return new CollectedPackages(used, restored, ignored);
    }

    /// <summary>
    /// A project that has an assets file but can't be read has unknown usage, so this stops the run instead of warning.
    /// </summary>
    private static void ReadAssets(string assetsFile, UsedPackages used)
    {
        try
        {
            using var stream = File.OpenRead(assetsFile);
            using var document = JsonDocument.Parse(stream);
            var root = document.RootElement;

            foreach (var library in root.GetProperty("libraries").EnumerateObject())
            {
                if (library.Value.GetProperty("type").GetString() == "package")
                {
                    used.AddPath(library.Value.GetProperty("path").GetString()!);
                }
            }

            if (!root.TryGetProperty("project", out var project)
                || !project.TryGetProperty("frameworks", out var frameworks))
                return;

            foreach (var framework in frameworks.EnumerateObject())
            {
                if (!framework.Value.TryGetProperty("downloadDependencies", out var downloads))
                    continue;

                foreach (var download in downloads.EnumerateArray())
                {
                    used.Add(download.GetProperty("name").GetString()!, download.GetProperty("version").GetString());
                }
            }
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException
                                      or KeyNotFoundException or InvalidOperationException)
        {
            throw new CleanerException($"Can't read {assetsFile}: {e.Message}");
        }
    }

    private static void ReadProjectSdks(string project, UsedPackages used, TextWriter log)
    {
        if (TryLoadXml(project, log) is not { } document)
        {
            return;
        }

        foreach (var element in document.Descendants())
        {
            switch (element.Name.LocalName)
            {
                case "Project" or "Import":
                    AddSdkList((string?)element.Attribute("Sdk"), used);
                    break;
                case "Sdk" when (string?)element.Attribute("Name") is { } name && element.Attribute("Version") is not null:
                    used.Add(name, (string?)element.Attribute("Version"));
                    break;
            }
        }
    }

    /// <summary>
    /// Reads a list like <c>A/1.0;B/2.0</c>. Entries with no version are built into the .NET SDK or get it from global.json.
    /// </summary>
    private static void AddSdkList(string? sdks, UsedPackages used)
    {
        foreach (var sdk in (sdks ?? "").Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (sdk.Split('/', 2) is [var name, var version])
            {
                used.Add(name, version);
            }
        }
    }

    private static void ReadPackageVersions(string props, UsedPackages used, TextWriter log)
    {
        if (TryLoadXml(props, log) is not { } document)
        {
            return;
        }

        foreach (var element in document.Descendants())
        {
            if (element.Name.LocalName is "PackageVersion" or "GlobalPackageReference"
                && (string?)element.Attribute("Include") is { } id)
            {
                used.Add(id, (string?)element.Attribute("Version"));
            }
        }
    }

    private static void ReadGlobalJson(string globalJson, UsedPackages used, TextWriter log)
    {
        ReadJson(globalJson, log, root =>
        {
            if (root.TryGetProperty("msbuild-sdks", out var sdks))
            {
                foreach (var sdk in sdks.EnumerateObject())
                {
                    used.Add(sdk.Name, sdk.Value.GetString());
                }
            }
        });
    }

    private static void ReadToolManifest(string manifest, UsedPackages used, TextWriter log)
    {
        ReadJson(manifest, log, root =>
        {
            if (root.TryGetProperty("tools", out var tools))
            {
                foreach (var tool in tools.EnumerateObject())
                {
                    used.Add(tool.Name, tool.Value.GetProperty("version").GetString());
                }
            }
        });
    }

    private static void ReadJson(string file, TextWriter log, Action<JsonElement> read)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file), _handWrittenJson);
            read(document.RootElement);
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException
                                      or KeyNotFoundException or InvalidOperationException)
        {
            log.WriteLine($"warning: skipping {file}: {e.Message}");
        }
    }

    private static XDocument? TryLoadXml(string file, TextWriter log)
    {
        try
        {
            return XDocument.Load(file);
        }
        catch (Exception e) when (e is XmlException or IOException or UnauthorizedAccessException)
        {
            log.WriteLine($"warning: skipping {file}: {e.Message}");
            return null;
        }
    }
}
