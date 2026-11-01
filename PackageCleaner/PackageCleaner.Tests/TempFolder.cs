namespace PackageCleaner.Tests;

public sealed class TempFolder : IDisposable
{
    public string Path { get; } = Directory.CreateTempSubdirectory("PackageCleaner.Tests").FullName;

    public string File(string relativePath, string content = "")
    {
        var path = Combine(relativePath);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        System.IO.File.WriteAllText(path, content);
        return path;
    }

    public string Folder(string relativePath) => Directory.CreateDirectory(Combine(relativePath)).FullName;

    public string Combine(string relativePath) => System.IO.Path.GetFullPath(relativePath, Path);

    /// <summary>Creates a restored project using the given <c>id/version</c> packages.</summary>
    public string RestoredProject(string relativePath, params string[] packages)
    {
        var libraries = packages.Select(package => $$"""
            "{{package}}": { "type": "package", "path": "{{package.ToLowerInvariant()}}" }
            """);
        var folder = System.IO.Path.GetDirectoryName(relativePath) ?? "";
        File(System.IO.Path.Combine(folder, "obj", "project.assets.json"), $$"""
            { "libraries": { {{string.Join(",", libraries)}} } }
            """);
        return File(relativePath, "<Project Sdk=\"Microsoft.NET.Sdk\" />");
    }

    public string CachedPackage(string id, string version)
    {
        File($"packages/{id}/{version}/.nupkg.metadata");
        File($"packages/{id}/{version}/lib/{id}.dll", "content");
        return Combine($"packages/{id}/{version}");
    }

    public void Dispose()
    {
        if (!Directory.Exists(Path))
        {
            return;
        }

        foreach (var file in new DirectoryInfo(Path).EnumerateFiles("*", SearchOption.AllDirectories))
        {
            file.IsReadOnly = false;
        }

        Directory.Delete(Path, recursive: true);
    }
}
