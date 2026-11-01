using PackageCleaner.NuGet;
using TUnit.Core.Enums;

namespace PackageCleaner.Tests;

public class PackageCacheTests
{
    [Test]
    public async Task Lists_only_real_package_folders()
    {
        using var temp = new TempFolder();
        var withMetadata = temp.CachedPackage("first", "1.0.0");
        var withHash = temp.File("packages/second/2.0.0/second.2.0.0.nupkg.sha512");
        temp.File("packages/third/3.0.0/readme.txt");
        temp.File("packages/loose.txt");

        var listed = PackageCache.List(temp.Combine("packages")).Select(package => package.Path).ToList();

        await Assert.That(listed).IsEquivalentTo([withMetadata, Path.GetDirectoryName(withHash)!]);
    }

    [Test]
    public async Task Lists_leftovers_of_interrupted_deletes()
    {
        using var temp = new TempFolder();
        temp.File("packages/first/.deleting-1.0.0/lib/a.dll");

        var package = PackageCache.List(temp.Combine("packages")).Single();

        await Assert.That(package.IsLeftover).IsTrue();
    }

    [Test]
    public async Task Size_counts_nested_files()
    {
        using var temp = new TempFolder();
        temp.File("package/a.txt", "12345");
        temp.File("package/lib/b.txt", "123");

        await Assert.That(PackageCache.GetSize(temp.Combine("package"))).IsEqualTo(8);
    }

    [Test]
    public async Task Delete_keeps_id_folder_while_other_versions_remain()
    {
        using var temp = new TempFolder();
        var old = temp.CachedPackage("first", "1.0.0");
        var kept = temp.CachedPackage("first", "2.0.0");

        PackageCache.Delete(old);

        await Assert.That(Directory.Exists(old)).IsFalse();
        await Assert.That(Directory.Exists(kept)).IsTrue();
        await Assert.That(Directory.GetDirectories(Path.GetDirectoryName(old)!)).Count().IsEqualTo(1);
    }

    [Test]
    public async Task Delete_removes_empty_id_folder()
    {
        using var temp = new TempFolder();
        var package = temp.CachedPackage("first", "1.0.0");

        PackageCache.Delete(package);

        await Assert.That(Directory.Exists(temp.Combine("packages/first"))).IsFalse();
    }

    [Test]
    public async Task Delete_handles_read_only_files()
    {
        using var temp = new TempFolder();
        var package = temp.CachedPackage("first", "1.0.0");
        new FileInfo(Path.Combine(package, ".nupkg.metadata")).IsReadOnly = true;

        PackageCache.Delete(package);

        await Assert.That(Directory.Exists(package)).IsFalse();
    }

    [Test]
    public async Task Delete_finishes_a_leftover()
    {
        using var temp = new TempFolder();
        temp.File("packages/first/.deleting-1.0.0/lib/a.dll");

        PackageCache.Delete(temp.Combine("packages/first/.deleting-1.0.0"));

        await Assert.That(Directory.Exists(temp.Combine("packages/first"))).IsFalse();
    }

    [Test]
    [RunOn(OS.Windows)]
    public async Task Locked_file_leaves_the_package_whole()
    {
        using var temp = new TempFolder();
        var package = temp.CachedPackage("first", "1.0.0");
        var dll = Path.Combine(package, "lib", "first.dll");

        using (File.Open(dll, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            await Assert.That(() => PackageCache.Delete(package)).Throws<Exception>();
        }

        await Assert.That(File.Exists(dll)).IsTrue();
        await Assert.That(File.Exists(Path.Combine(package, ".nupkg.metadata"))).IsTrue();
    }
}
