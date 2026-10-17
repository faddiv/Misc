namespace SqlParsers;

/// <summary>Reserved words of the supported SQL-92 SELECT subset. Matching is case-insensitive.</summary>
public static class SqlKeywords
{
    public static readonly IReadOnlyList<string> Reserved =
    [
        "ALL", "AND", "ANY", "AS", "ASC", "BETWEEN", "BY", "CASE", "CAST", "CROSS",
        "DESC", "DISTINCT", "ELSE", "END", "ESCAPE", "EXCEPT", "EXISTS", "FROM", "FULL", "GROUP",
        "HAVING", "IN", "INNER", "INTERSECT", "IS", "JOIN", "LEFT", "LIKE", "NATURAL", "NOT",
        "NULL", "ON", "OR", "ORDER", "OUTER", "RIGHT", "SELECT", "SOME", "THEN", "UNION",
        "USING", "WHEN", "WHERE",
    ];

    private static readonly HashSet<string> Set = new(Reserved, StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> SpanLookup =
        Set.GetAlternateLookup<ReadOnlySpan<char>>();

    public static bool IsReserved(string word) => Set.Contains(word);

    public static bool IsReserved(ReadOnlySpan<char> word) => SpanLookup.Contains(word);
}
