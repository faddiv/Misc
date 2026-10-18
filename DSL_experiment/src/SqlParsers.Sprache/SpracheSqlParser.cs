using Sprache;
using SqlParsers.Ast;

namespace SqlParsers.Sprache;

/// <summary>Scannerless, fully backtracking parser built with Sprache combinators. The grammar lives in <c>Grammar/</c>.</summary>
public sealed class SpracheSqlParser : ISqlParser
{
    public string Name => "Sprache";

    public SelectStatement Parse(string sql)
    {
        var result = SpracheGrammar.Statement(SourceInput.Start(sql));
        return result.WasSuccessful ? result.Value : throw new SqlParseException(result.ToString() ?? "Invalid SQL");
    }
}
