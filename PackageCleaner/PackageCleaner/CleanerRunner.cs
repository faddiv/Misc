namespace PackageCleaner;

/// <summary>Scans with every cleaner, then shows one combined list and asks once before deleting anything.</summary>
public sealed class CleanerRunner(TextWriter output, TextReader input, bool isInteractive)
{
    private readonly TextWriter _output = output;
    private readonly TextReader _input = input;
    private readonly bool _isInteractive = isInteractive;

    public int Run(IReadOnlyList<ICleaner> cleaners, IReadOnlyList<string> folders, bool dryRun, bool yes)
    {
        var items = new List<(ICleaner Cleaner, CleanupItem Item)>();
        try
        {
            foreach (var cleaner in cleaners)
            {
                items.AddRange(cleaner.Scan(folders).Select(item => (cleaner, item)));
            }
        }
        catch (CleanerException e)
        {
            _output.WriteLine($"error: {e.Message} Nothing was deleted.");
            return 1;
        }

        if (items.Count == 0)
        {
            _output.WriteLine("Nothing to clean.");
            return 0;
        }

        foreach (var (cleaner, item) in items)
        {
            _output.WriteLine($"{cleaner.Name}: {item.Name} ({FormatSize(item.Size)})");
        }

        var summary = $"{items.Count} items, {FormatSize(items.Sum(entry => entry.Item.Size))}";
        _output.WriteLine($"Total: {summary}");

        if (dryRun)
        {
            return 0;
        }

        if (!yes)
        {
            if (!_isInteractive)
            {
                _output.WriteLine("error: Input is not interactive, pass --yes to delete. Nothing was deleted.");
                return 1;
            }

            _output.Write($"Delete {summary}? [y/N] ");
            if (_input.ReadLine()?.Trim().ToLowerInvariant() is not ("y" or "yes"))
            {
                _output.WriteLine("Nothing was deleted.");
                return 0;
            }
        }

        return Delete(items);
    }

    private int Delete(List<(ICleaner Cleaner, CleanupItem Item)> items)
    {
        var failed = 0;
        long freed = 0;
        foreach (var (cleaner, item) in items)
        {
            try
            {
                cleaner.Delete(item);
                freed += item.Size;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                _output.WriteLine($"warning: can't delete {item.Name}: {e.Message}");
                failed++;
            }
        }

        _output.WriteLine($"Deleted {items.Count - failed} items, freed {FormatSize(freed)}. Failed: {failed}.");
        return failed == 0 ? 0 : 1;
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F2} GB",
    };
}
