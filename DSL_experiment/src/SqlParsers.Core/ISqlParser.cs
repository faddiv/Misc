using SqlParsers.Ast;

namespace SqlParsers;

public interface ISqlParser
{
    /// <summary>Display name used in tests and benchmark parameters.</summary>
    string Name { get; }

    /// <summary>Parses one SQL-92 SELECT statement.</summary>
    /// <exception cref="SqlParseException">The input is not a valid statement.</exception>
    SelectStatement Parse(string sql);
}

public sealed class SqlParseException(string message, Exception? inner = null) : Exception(message, inner);
