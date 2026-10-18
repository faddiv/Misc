using Pidgin;
using SqlParsers.Ast;

namespace SqlParsers.Pidgin;

/// <summary>Scannerless parser built with Pidgin combinators. The grammar lives in <c>Grammar/</c>.</summary>
public sealed class PidginSqlParser : ISqlParser
{
    public string Name => "Pidgin";

    public SelectStatement Parse(string sql)
    {
        var result = PidginGrammar.Statement.Parse(sql);
        return result.Success
            ? result.Value
            : throw new SqlParseException(result.Error?.ToString() ?? "Invalid SQL");
    }
}
