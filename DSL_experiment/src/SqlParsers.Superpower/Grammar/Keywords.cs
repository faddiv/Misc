using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SqlParsers.Superpower;

/// <summary>The reserved words of the grammar, one parser each (e.g. <c>K.Select</c>). The tokenizer gives every keyword its own token kind.</summary>
internal static class K
{
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> All = Lex.Kind(SqlToken.All);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> And = Lex.Kind(SqlToken.And);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Any = Lex.Kind(SqlToken.Any);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> As = Lex.Kind(SqlToken.As);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Asc = Lex.Kind(SqlToken.Asc);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Between = Lex.Kind(SqlToken.Between);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> By = Lex.Kind(SqlToken.By);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Case = Lex.Kind(SqlToken.Case);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Cast = Lex.Kind(SqlToken.Cast);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Cross = Lex.Kind(SqlToken.Cross);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Desc = Lex.Kind(SqlToken.Desc);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Distinct = Lex.Kind(SqlToken.Distinct);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Else = Lex.Kind(SqlToken.Else);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> End = Lex.Kind(SqlToken.End);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Escape = Lex.Kind(SqlToken.Escape);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Except = Lex.Kind(SqlToken.Except);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Exists = Lex.Kind(SqlToken.Exists);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> From = Lex.Kind(SqlToken.From);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Full = Lex.Kind(SqlToken.Full);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Group = Lex.Kind(SqlToken.Group);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Having = Lex.Kind(SqlToken.Having);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> In = Lex.Kind(SqlToken.In);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Inner = Lex.Kind(SqlToken.Inner);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Intersect = Lex.Kind(SqlToken.Intersect);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Is = Lex.Kind(SqlToken.Is);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Join = Lex.Kind(SqlToken.Join);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Left = Lex.Kind(SqlToken.Left);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Like = Lex.Kind(SqlToken.Like);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Natural = Lex.Kind(SqlToken.Natural);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Not = Lex.Kind(SqlToken.Not);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Null = Lex.Kind(SqlToken.Null);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> On = Lex.Kind(SqlToken.On);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Or = Lex.Kind(SqlToken.Or);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Order = Lex.Kind(SqlToken.Order);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Outer = Lex.Kind(SqlToken.Outer);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Right = Lex.Kind(SqlToken.Right);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Select = Lex.Kind(SqlToken.Select);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Some = Lex.Kind(SqlToken.Some);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Then = Lex.Kind(SqlToken.Then);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Union = Lex.Kind(SqlToken.Union);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Using = Lex.Kind(SqlToken.Using);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> When = Lex.Kind(SqlToken.When);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Where = Lex.Kind(SqlToken.Where);
}
