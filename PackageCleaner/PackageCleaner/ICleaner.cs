namespace PackageCleaner;

public interface ICleaner
{
    string Name { get; }

    /// <summary>Finds what can be removed, without changing anything.</summary>
    /// <exception cref="CleanerException">The result can't be trusted, so the run has to stop.</exception>
    IReadOnlyList<CleanupItem> Scan(IReadOnlyList<string> folders);

    /// <exception cref="IOException">The item stays and the run continues with the next one.</exception>
    /// <exception cref="UnauthorizedAccessException">The item stays and the run continues with the next one.</exception>
    void Delete(CleanupItem item);
}
