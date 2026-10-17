namespace SqlParsers.Tests;

public class ParserTests
{
    public static IEnumerable<(string Parser, string Case)> ValidCases() =>
        from parser in ParserCatalog.Names
        from c in TestQueries.Valid
        select (parser, c.Name);

    public static IEnumerable<(string Parser, string Sql)> InvalidCases() =>
        from parser in ParserCatalog.Names
        from sql in TestQueries.Invalid
        select (parser, sql);

    public static IEnumerable<string> SampleQueryNames() => SampleQueries.All.Keys;

    [Test]
    [MethodDataSource(nameof(ValidCases))]
    public async Task Parses_to_expected_tree(string parser, string caseName)
    {
        var testCase = TestQueries.Valid.Single(c => c.Name == caseName);

        var statement = ParserCatalog.Get(parser).Parse(testCase.Sql);

        await Assert.That(SqlFormatter.Format(statement)).IsEqualTo(testCase.Expected);
    }

    [Test]
    [MethodDataSource(nameof(InvalidCases))]
    public async Task Rejects_invalid_sql(string parser, string sql)
    {
        await Assert.That(() => ParserCatalog.Get(parser).Parse(sql)).Throws<SqlParseException>();
    }

    /// <summary>The benchmark inputs must produce identical trees, otherwise the comparison is unfair.</summary>
    [Test]
    [MethodDataSource(nameof(SampleQueryNames))]
    public async Task All_parsers_agree_on_benchmark_queries(string queryName)
    {
        var sql = SampleQueries.All[queryName];

        var results = ParserCatalog.All
            .Select(p => (p.Name, Tree: SqlFormatter.Format(p.Parse(sql))))
            .ToList();

        var reference = results[0];
        var mismatches = string.Join(
            Environment.NewLine,
            results.Where(r => r.Tree != reference.Tree).Select(r => $"{r.Name} differs from {reference.Name}: {r.Tree}"));

        await Assert.That(mismatches).IsEqualTo(string.Empty);
    }
}
