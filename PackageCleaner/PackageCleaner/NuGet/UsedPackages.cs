using NuGet.Versioning;

namespace PackageCleaner.NuGet;

/// <summary>The set of packages that must stay in the cache.</summary>
public sealed class UsedPackages
{
    private readonly HashSet<string> _exact = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<VersionRange>> _ranges = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _wholeIds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Adds a package by its cache path, such as <c>newtonsoft.json/13.0.3</c>.</summary>
    public void AddPath(string path) => _exact.Add(path);

    /// <summary>
    /// Adds a package by a version as written in a project: a plain version keeps exactly that version, a floating or
    /// bracket range keeps every version in it, and anything else, like <c>$(SomeVersion)</c>, keeps the whole ID.
    /// </summary>
    public void Add(string id, string? version)
    {
        if (NuGetVersion.TryParse(version, out var exact))
        {
            _exact.Add($"{id}/{exact.ToNormalizedString()}");
        }
        else if (VersionRange.TryParse(version ?? "", allowFloating: true, out var range))
        {
            if (!_ranges.TryGetValue(id, out var ranges))
            {
                _ranges[id] = ranges = [];
            }

            ranges.Add(range);
        }
        else
        {
            _wholeIds.Add(id);
        }
    }

    public bool IsUsed(string id, string version)
    {
        if (_wholeIds.Contains(id) || _exact.Contains($"{id}/{version}"))
        {
            return true;
        }

        return _ranges.TryGetValue(id, out var ranges)
            && NuGetVersion.TryParse(version, out var parsed)
            && ranges.Any(range => range.IsFloating ? range.Float.Satisfies(parsed) : range.Satisfies(parsed));
    }
}
