using SqlParsers.Antlr;
using SqlParsers.Irony;
using SqlParsers.Parlot;
using SqlParsers.Pidgin;
using SqlParsers.Sprache;
using SqlParsers.Superpower;

namespace SqlParsers.Tests;

public static class ParserCatalog
{
    public static readonly IReadOnlyList<ISqlParser> All =
    [
        new PidginSqlParser(),
        new SuperpowerSqlParser(),
        new ParlotSqlParser(),
        new SpracheSqlParser(),
        new AntlrSqlParser(),
        new IronySqlParser(),
    ];

    public static IEnumerable<string> Names => All.Select(p => p.Name);

    public static ISqlParser Get(string name) => All.Single(p => p.Name == name);
}
