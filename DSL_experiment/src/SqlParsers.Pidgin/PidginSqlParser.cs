using Pidgin;
using Pidgin.Configuration;
using SqlParsers.Ast;

namespace SqlParsers.Pidgin;

/// <summary>Scannerless parser built with Pidgin combinators. The grammar lives in <c>Grammar/</c>.</summary>
public sealed class PidginSqlParser : ISqlParser
{
    // Per-thread array pools instead of ArrayPool<T>.Shared (see ThreadArrayPoolProvider).
    private static readonly IConfiguration<char> Configuration =
        global::Pidgin.Configuration.Configuration.Default<char>().WithArrayPoolProvider(ThreadArrayPoolProvider.Instance);

    public string Name => "Pidgin";

    public SelectStatement Parse(string sql)
    {
        var result = PidginGrammar.Statement.Parse(sql, Configuration);
        return result.Success
            ? result.Value
            : throw new SqlParseException(result.Error?.ToString() ?? "Invalid SQL");
    }
}
