using PackageCleaner.NuGet;

namespace PackageCleaner.Tests;

public class UsedPackagesTests
{
    [Test]
    [Arguments("1.2.3", "1.2.3", true)]
    [Arguments("1.2.3", "1.2.4", false)]
    [Arguments("1.2.3.0", "1.2.3", true)]
    [Arguments("1.2", "1.2.0", true)]
    [Arguments("1.2.3+build", "1.2.3", true)]
    [Arguments("1.2.3-Beta", "1.2.3-beta", true)]
    [Arguments("1.*", "1.5.0", true)]
    [Arguments("1.*", "2.0.0", false)]
    [Arguments("1.2.*", "1.2.9", true)]
    [Arguments("1.2.*", "1.3.0", false)]
    [Arguments("[1.0,2.0)", "1.9.0", true)]
    [Arguments("[1.0,2.0)", "2.0.0", false)]
    [Arguments("[8.0.22, 8.0.22]", "8.0.22", true)]
    [Arguments("[8.0.22, 8.0.22]", "8.0.23", false)]
    [Arguments("$(SomeVersion)", "9.9.9", true)]
    [Arguments("", "9.9.9", true)]
    [Arguments(null, "9.9.9", true)]
    public async Task Version_as_written_in_a_project(string? written, string cached, bool expected)
    {
        var used = new UsedPackages();

        used.Add("Some.Package", written);

        await Assert.That(used.IsUsed("some.package", cached)).IsEqualTo(expected);
    }

    [Test]
    public async Task Cache_path_matches_exactly()
    {
        var used = new UsedPackages();

        used.AddPath("some.package/1.2.3");

        await Assert.That(used.IsUsed("some.package", "1.2.3")).IsTrue();
        await Assert.That(used.IsUsed("some.package", "1.2.4")).IsFalse();
        await Assert.That(used.IsUsed("other.package", "1.2.3")).IsFalse();
    }
}
