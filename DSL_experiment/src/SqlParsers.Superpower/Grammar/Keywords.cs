using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SqlParsers.Superpower;

/// <summary>The reserved words of the grammar, one parser each (e.g. <c>K.Select</c>): a Word token with that text.</summary>
internal static class K
{
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> All = Token.EqualToValueIgnoreCase(SqlToken.Word, "ALL");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> And = Token.EqualToValueIgnoreCase(SqlToken.Word, "AND");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Any = Token.EqualToValueIgnoreCase(SqlToken.Word, "ANY");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> As = Token.EqualToValueIgnoreCase(SqlToken.Word, "AS");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Asc = Token.EqualToValueIgnoreCase(SqlToken.Word, "ASC");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Between = Token.EqualToValueIgnoreCase(SqlToken.Word, "BETWEEN");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> By = Token.EqualToValueIgnoreCase(SqlToken.Word, "BY");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Case = Token.EqualToValueIgnoreCase(SqlToken.Word, "CASE");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Cast = Token.EqualToValueIgnoreCase(SqlToken.Word, "CAST");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Cross = Token.EqualToValueIgnoreCase(SqlToken.Word, "CROSS");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Desc = Token.EqualToValueIgnoreCase(SqlToken.Word, "DESC");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Distinct = Token.EqualToValueIgnoreCase(SqlToken.Word, "DISTINCT");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Else = Token.EqualToValueIgnoreCase(SqlToken.Word, "ELSE");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> End = Token.EqualToValueIgnoreCase(SqlToken.Word, "END");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Escape = Token.EqualToValueIgnoreCase(SqlToken.Word, "ESCAPE");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Except = Token.EqualToValueIgnoreCase(SqlToken.Word, "EXCEPT");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Exists = Token.EqualToValueIgnoreCase(SqlToken.Word, "EXISTS");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> From = Token.EqualToValueIgnoreCase(SqlToken.Word, "FROM");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Full = Token.EqualToValueIgnoreCase(SqlToken.Word, "FULL");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Group = Token.EqualToValueIgnoreCase(SqlToken.Word, "GROUP");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Having = Token.EqualToValueIgnoreCase(SqlToken.Word, "HAVING");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> In = Token.EqualToValueIgnoreCase(SqlToken.Word, "IN");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Inner = Token.EqualToValueIgnoreCase(SqlToken.Word, "INNER");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Intersect = Token.EqualToValueIgnoreCase(SqlToken.Word, "INTERSECT");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Is = Token.EqualToValueIgnoreCase(SqlToken.Word, "IS");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Join = Token.EqualToValueIgnoreCase(SqlToken.Word, "JOIN");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Left = Token.EqualToValueIgnoreCase(SqlToken.Word, "LEFT");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Like = Token.EqualToValueIgnoreCase(SqlToken.Word, "LIKE");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Natural = Token.EqualToValueIgnoreCase(SqlToken.Word, "NATURAL");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Not = Token.EqualToValueIgnoreCase(SqlToken.Word, "NOT");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Null = Token.EqualToValueIgnoreCase(SqlToken.Word, "NULL");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> On = Token.EqualToValueIgnoreCase(SqlToken.Word, "ON");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Or = Token.EqualToValueIgnoreCase(SqlToken.Word, "OR");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Order = Token.EqualToValueIgnoreCase(SqlToken.Word, "ORDER");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Outer = Token.EqualToValueIgnoreCase(SqlToken.Word, "OUTER");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Right = Token.EqualToValueIgnoreCase(SqlToken.Word, "RIGHT");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Select = Token.EqualToValueIgnoreCase(SqlToken.Word, "SELECT");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Some = Token.EqualToValueIgnoreCase(SqlToken.Word, "SOME");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Then = Token.EqualToValueIgnoreCase(SqlToken.Word, "THEN");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Union = Token.EqualToValueIgnoreCase(SqlToken.Word, "UNION");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Using = Token.EqualToValueIgnoreCase(SqlToken.Word, "USING");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> When = Token.EqualToValueIgnoreCase(SqlToken.Word, "WHEN");
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Where = Token.EqualToValueIgnoreCase(SqlToken.Word, "WHERE");
}
