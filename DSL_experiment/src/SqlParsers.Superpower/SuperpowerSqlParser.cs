using SqlParsers.Ast;
using Superpower;

namespace SqlParsers.Superpower;

/// <summary>Two-phase parser: Superpower tokenizer followed by token-list combinators. The grammar lives in <c>Grammar/</c>.</summary>
public sealed class SuperpowerSqlParser : ISqlParser
{
    public string Name => "Superpower";

    public SelectStatement Parse(string sql)
    {
        var tokens = Lex.Tokenizer.TryTokenize(sql);
        if (!tokens.HasValue)
            throw new SqlParseException(tokens.ToString());

        var result = SuperpowerGrammar.Statement.TryParse(tokens.Value);
        return result.HasValue ? result.Value : throw new SqlParseException(result.ToString());
    }
}
