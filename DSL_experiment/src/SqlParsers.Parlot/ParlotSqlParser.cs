using SqlParsers.Ast;

namespace SqlParsers.Parlot;

/// <summary>Scannerless parser built with Parlot's fluent combinators. The grammar lives in <c>Grammar/</c>.</summary>
public sealed class ParlotSqlParser : ISqlParser
{
    public string Name => "Parlot";

    public SelectStatement Parse(string sql)
    {
        if (ParlotGrammar.Statement.TryParse(sql, out var result, out var error) && result is not null)
            return result;

        throw new SqlParseException(error is null ? "Invalid SQL" : $"{error.Message} at {error.Position}");
    }
}
