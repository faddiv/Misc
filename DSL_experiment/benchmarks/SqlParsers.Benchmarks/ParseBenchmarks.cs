using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using SqlParsers.Antlr;
using SqlParsers.Ast;
using SqlParsers.Irony;
using SqlParsers.Parlot;
using SqlParsers.Pidgin;
using SqlParsers.Sprache;
using SqlParsers.Superpower;

namespace SqlParsers.Benchmarks;

/// <summary>
/// Time and allocations for parsing one statement into the shared AST, per library and per query size.
/// Parser construction (grammar/table building) happens in GlobalSetup and is not measured.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
public class ParseBenchmarks
{
    private static readonly IReadOnlyDictionary<string, Func<ISqlParser>> Parsers = new Dictionary<string, Func<ISqlParser>>
    {
        ["Pidgin"] = () => new PidginSqlParser(),
        ["Superpower"] = () => new SuperpowerSqlParser(),
        ["Parlot"] = () => new ParlotSqlParser(),
        ["Sprache"] = () => new SpracheSqlParser(),
        ["ANTLR"] = () => new AntlrSqlParser(),
        ["Irony"] = () => new IronySqlParser(),
    };

    private ISqlParser _parser = null!;
    private string _sql = null!;

    public static IEnumerable<string> QueryNames => SampleQueries.All.Keys;

    public static IEnumerable<string> ParserNames => Parsers.Keys;

    [ParamsSource(nameof(QueryNames))]
    public string Query { get; set; } = "";

    [ParamsSource(nameof(ParserNames))]
    public string Parser { get; set; } = "";

    [GlobalSetup]
    public void Setup()
    {
        _parser = Parsers[Parser]();
        _sql = SampleQueries.All[Query];
        _parser.Parse(_sql); // fail fast if the input is not accepted; also warms static grammar state
    }

    [Benchmark]
    public SelectStatement Parse() => _parser.Parse(_sql);
}
