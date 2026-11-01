using System.CommandLine;
using PackageCleaner.NuGet;

namespace PackageCleaner;

public static class CommandLineApp
{
    public static RootCommand Build(TextWriter output, TextReader input, bool isInteractive)
    {
        var dryRun = new Option<bool>("--dry-run")
        {
            Description = "List what would be deleted, then stop.",
            Recursive = true,
        };
        var yes = new Option<bool>("--yes", "-y")
        {
            Description = "Delete without asking for confirmation.",
            Recursive = true,
        };
        var packagesPath = new Option<DirectoryInfo>("--packages-path")
        {
            Description = "NuGet packages folder to clean, instead of the one NuGet is configured to use.",
            Recursive = true,
        }.AcceptExistingOnly();

        var rootFolders = CreateFoldersArgument();
        var nugetFolders = CreateFoldersArgument();

        var nuget = new Command("nuget", "Run the NuGet cleaner only.") { nugetFolders };
        var root = new RootCommand("Removes what the projects under the given folders no longer use. Runs every cleaner.")
        {
            rootFolders, dryRun, yes, packagesPath, nuget,
        };

        ICleaner CreateNuGet(ParseResult result) => new NuGetCleaner(result.GetValue(packagesPath)?.FullName, output);

        int Run(ParseResult result, Argument<DirectoryInfo[]> folders, params ICleaner[] cleaners) =>
            new CleanerRunner(output, input, isInteractive).Run(
                cleaners,
                result.GetRequiredValue(folders).Select(folder => folder.FullName).ToList(),
                result.GetValue(dryRun),
                result.GetValue(yes));

        // Every new cleaner goes into the root action and gets its own subcommand.
        root.SetAction(result => Run(result, rootFolders, CreateNuGet(result)));
        nuget.SetAction(result => Run(result, nugetFolders, CreateNuGet(result)));

        return root;
    }

    private static Argument<DirectoryInfo[]> CreateFoldersArgument() =>
        new Argument<DirectoryInfo[]>("folders")
        {
            Description = "Folders to scan for projects. Include every folder that has projects to keep packages for.",
            Arity = ArgumentArity.OneOrMore,
        }.AcceptExistingOnly();
}
