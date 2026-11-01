namespace PackageCleaner.NuGet;

public sealed record ScanResult(
    IReadOnlyList<string> ProjectFiles,
    IReadOnlyList<string> PackagePropsFiles,
    IReadOnlyList<string> GlobalJsonFiles,
    IReadOnlyList<string> ToolManifests);

/// <summary>Finds the files that describe package usage below the given folders.</summary>
public static class ProjectScanner
{
    private static readonly HashSet<string> SkippedFolders =
        new([".git", "node_modules", "bin", "obj", ".vs", ".idea"], StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> ProjectExtensions =
        new([".csproj", ".fsproj", ".vbproj"], StringComparer.OrdinalIgnoreCase);

    public static ScanResult Scan(IEnumerable<string> folders, TextWriter log)
    {
        var projects = new List<string>();
        var props = new List<string>();
        var globalJsons = new List<string>();
        var manifests = new List<string>();

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Stack<string>(folders.Select(Path.GetFullPath));

        while (pending.TryPop(out var folder))
        {
            if (!visited.Add(folder))
            {
                continue;
            }

            FileSystemInfo[] entries;
            try
            {
                entries = new DirectoryInfo(folder).GetFileSystemInfos();
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                log.WriteLine($"warning: skipping {folder}: {e.Message}");
                continue;
            }

            var hasProject = false;
            foreach (var file in entries.OfType<FileInfo>())
            {
                if (ProjectExtensions.Contains(file.Extension))
                {
                    projects.Add(file.FullName);
                    hasProject = true;
                }
                else if (file.Name.Equals("Directory.Packages.props", StringComparison.OrdinalIgnoreCase))
                {
                    props.Add(file.FullName);
                }
                else if (file.Name.Equals("global.json", StringComparison.OrdinalIgnoreCase))
                {
                    globalJsons.Add(file.FullName);
                }
                else if (file.Name.Equals("dotnet-tools.json", StringComparison.OrdinalIgnoreCase))
                {
                    manifests.Add(file.FullName);
                }
            }

            if (hasProject)
            {
                // Recursion stops here, so it won't reach the .config folder.
                var configManifest = Path.Combine(folder, ".config", "dotnet-tools.json");
                if (File.Exists(configManifest))
                {
                    manifests.Add(configManifest);
                }

                continue;
            }

            foreach (var directory in entries.OfType<DirectoryInfo>())
            {
                if (!SkippedFolders.Contains(directory.Name) && !directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    pending.Push(directory.FullName);
                }
            }
        }

        return new ScanResult(projects, props, globalJsons, manifests);
    }
}
