namespace PackageCleaner.NuGet;

public sealed record CachedPackage(string Id, string Version, string Path)
{
    public bool IsLeftover => Version.StartsWith(PackageCache.DeletingPrefix, StringComparison.Ordinal);
}

/// <summary>The global packages folder, laid out as <c>id/version/</c>.</summary>
public static class PackageCache
{
    public const string DeletingPrefix = ".deleting-";

    /// <summary>
    /// Lists packages and the leftovers of interrupted deletes. Anything else is not listed, so it can't be deleted.
    /// </summary>
    public static IEnumerable<CachedPackage> List(string packagesPath)
    {
        foreach (var idFolder in Directory.EnumerateDirectories(packagesPath))
        {
            foreach (var versionFolder in Directory.EnumerateDirectories(idFolder))
            {
                var version = Path.GetFileName(versionFolder);
                if (version.StartsWith(DeletingPrefix, StringComparison.Ordinal) || IsPackageFolder(versionFolder))
                {
                    yield return new CachedPackage(Path.GetFileName(idFolder), version, versionFolder);
                }
            }
        }
    }

    public static long GetSize(string packageFolder) =>
        new DirectoryInfo(packageFolder).EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);

    /// <summary>
    /// Renames the folder first: this fails while any file in it is open, which leaves the package whole. NuGet also won't
    /// find a half-deleted package under the renamed folder if the delete itself gets interrupted.
    /// </summary>
    public static void Delete(string packageFolder)
    {
        var idFolder = Path.GetDirectoryName(packageFolder)!;
        var version = Path.GetFileName(packageFolder);

        var deleting = packageFolder;
        if (!version.StartsWith(DeletingPrefix, StringComparison.Ordinal))
        {
            deleting = Path.Combine(idFolder, DeletingPrefix + version);
            Directory.Move(packageFolder, deleting);
        }

        foreach (var file in new DirectoryInfo(deleting).EnumerateFiles("*", SearchOption.AllDirectories))
        {
            file.IsReadOnly = false;
        }

        Directory.Delete(deleting, recursive: true);

        if (!Directory.EnumerateFileSystemEntries(idFolder).Any())
        {
            Directory.Delete(idFolder);
        }
    }

    private static bool IsPackageFolder(string folder) =>
        File.Exists(Path.Combine(folder, ".nupkg.metadata"))
        || Directory.EnumerateFiles(folder, "*.nupkg.sha512").Any();
}
