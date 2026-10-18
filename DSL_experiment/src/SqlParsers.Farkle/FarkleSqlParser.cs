using Farkle;
using SqlParsers.Ast;

namespace SqlParsers.Farkle;

/// <summary>
/// Farkle 7: grammar written in C# with a combinator-like API, compiled at startup into LALR(1)/IELR(1) tables and a
/// DFA tokenizer. Semantic actions build the shared AST directly. The grammar lives in <c>Grammar/</c>.
/// </summary>
public sealed class FarkleSqlParser : ISqlParser
{
    public string Name => "Farkle";

    public SelectStatement Parse(string sql)
    {
        var result = FarkleGrammar.Parser.Parse(sql);
        return result.IsSuccess ? result.Value : throw new SqlParseException(result.Error?.ToString() ?? "Invalid SQL");
    }
}
