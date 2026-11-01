using NuGet.Configuration;

namespace PackageCleaner.NuGet;

/// <summary>Removes packages from the global packages folder that no project under the scanned folders uses.</summary>
public sealed class NuGetCleaner(string? packagesPath, TextWriter log) : ICleaner
{
    private readonly TextWriter _log = log;
    private readonly string? _packagesPath = packagesPath;
    public string Name => "nuget";

    public IReadOnlyList<CleanupItem> Scan(IReadOnlyList<string> folders)
    {
        var collected = UsedPackagesCollector.Collect(ProjectScanner.Scan(folders, _log), _log);

        if (collected.RestoredProjects.Count == 0)
        {
            throw new CleanerException(
                "No restored project found, so nothing can be told apart from unused packages. Check the folders.");
        }

        _log.WriteLine($"Restored projects: {collected.RestoredProjects.Count}");
        foreach (var project in collected.IgnoredProjects)
        {
            _log.WriteLine($"Ignored, not restored: {project}");
        }

        var path = _packagesPath ?? SettingsUtility.GetGlobalPackagesFolder(
            Settings.LoadDefaultSettings(Directory.GetCurrentDirectory()));
        _log.WriteLine($"Packages folder: {path}");

        // Leftovers go first, because deleting a re-downloaded version needs the name a leftover may hold.
        return PackageCache.List(path)
            .Where(package => package.IsLeftover || !collected.Used.IsUsed(package.Id, package.Version))
            .OrderByDescending(package => package.IsLeftover)
            .ThenBy(package => package.Id, StringComparer.Ordinal)
            .ThenBy(package => package.Version, StringComparer.Ordinal)
            .Select(package => new CleanupItem(
                $"{package.Id} {package.Version}", package.Path, PackageCache.GetSize(package.Path)))
            .ToList();
    }

    public void Delete(CleanupItem item) => PackageCache.Delete(item.Path);
}
