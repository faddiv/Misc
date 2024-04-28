using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace SourceGeneratorTests.TestInfrastructure;

internal class TestEnvironment
{
    private static readonly string _projectDirectory;

    private static readonly string _validTestDataDirectory;


    private static readonly string _invalidTestDataDirectory;

    public static readonly string AttributeImpl;

    static TestEnvironment()
    {
        _projectDirectory = FindDirectoryOfFile(".csproj");
        _validTestDataDirectory = Path.Combine(_projectDirectory, "SourceGenerationTestCases");
        _invalidTestDataDirectory = Path.Combine(_projectDirectory, "ErrorReportingTestCases");
        AttributeImpl = File.ReadAllText(Path.Combine(_validTestDataDirectory, "Attribute.cs"));
    }
    public static string GetValidSource([CallerMemberName] string caller = null)
    {
        var sourcePath = Path.Combine(_validTestDataDirectory, caller, "_source.cs");
        return File.ReadAllText(sourcePath);
    }
    public static string GetInvalidSource([CallerMemberName] string caller = null)
    {
        var sourcePath = Path.Combine(_invalidTestDataDirectory, caller, "_source.cs");
        return File.ReadAllText(sourcePath);
    }

    public static (string filename, string content) GetDefaultOuput()
    {
        return ("ParamsAttribute.g.cs", AttributeImpl);
    }
    public static (string filename, string content)[] GetOuputs([CallerMemberName] string caller = null)
    {
        var basePath = Path.Combine(_validTestDataDirectory, caller);
        var sources = new List<(string filename, string content)>
        {
            GetDefaultOuput()
        };
        foreach (var filePath in Directory.GetFiles(basePath, "*.cs", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileName(filePath);
            if (fileName == "_source.cs")
                continue;

            var content = File.ReadAllText(filePath);
            sources.Add((fileName, content));
        }
        return sources.ToArray();
    }

    private static string FindDirectoryOfFile(string fileExtension, [CallerFilePath] string baseFilePath = null)
    {
        var dir =
            Path.GetDirectoryName(baseFilePath) ?? throw new InvalidOperationException($"Could not get directory from {baseFilePath}");

        while (Directory.GetFiles(dir, $"*{fileExtension}", SearchOption.TopDirectoryOnly).Length == 0)
        {
            dir = Path.GetDirectoryName(dir);
            if (dir == null)
            {
                throw new InvalidOperationException($"Could not find directory from file {baseFilePath}");
            }
        }

        return dir;
    }
}
