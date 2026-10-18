using System.Globalization;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SqlParsers.Superpower;

public enum SqlToken
{
    Identifier,
    QuotedIdentifier,
    Number,
    String,
    LParen,
    RParen,
    Comma,
    Dot,
    Semicolon,
    Star,
    Plus,
    Minus,
    Slash,
    Concat,
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,

    // Keywords: one kind each, so a keyword parser is a single comparison (see K).
    All,
    And,
    Any,
    As,
    Asc,
    Between,
    By,
    Case,
    Cast,
    Cross,
    Desc,
    Distinct,
    Else,
    End,
    Escape,
    Except,
    Exists,
    From,
    Full,
    Group,
    Having,
    In,
    Inner,
    Intersect,
    Is,
    Join,
    Left,
    Like,
    Natural,
    Not,
    Null,
    On,
    Or,
    Order,
    Outer,
    Right,
    Select,
    Some,
    Then,
    Union,
    Using,
    When,
    Where,
}

/// <summary>
/// Lexical rules of <c>grammar/sql92-select.ebnf</c>. Superpower tokenizes first, so this class holds the
/// tokenizer (<see cref="SqlTokenizer"/>) and one parser per token. Keywords have their own token kinds (see <see cref="K"/>).
/// </summary>
internal static class Lex
{
    public static readonly Tokenizer<SqlToken> Tokenizer = new SqlTokenizer();

    /// <summary>
    /// A token of the given kind. This is <c>Token.EqualTo(kind)</c> without its cost: Superpower compares kinds
    /// with <c>Equals</c> on the generic <c>TKind</c>, which boxes the enum values (48 bytes per comparison).
    /// Every keyword, operator and punctuation parser is one of these, so that was most of the allocations.
    /// </summary>
    public static TokenListParser<SqlToken, Token<SqlToken>> Kind(SqlToken kind)
    {
        var expectations = new[] { kind.ToString().ToLowerInvariant() };
        return input =>
        {
            var next = input.ConsumeToken();
            return next.HasValue && next.Value.Kind == kind
                ? next
                : TokenListParserResult.Empty<SqlToken, Token<SqlToken>>(input, expectations);
        };
    }

    /// <summary>
    /// One token out of several, mapped to a value: <c>(a | b | c)</c> where each alternative is a single token.
    /// Written with <c>Or</c>, every alternative that fails merges Superpower's "expected" lists (an allocation),
    /// and operators are tried after every operand, where they usually fail. This checks the token once.
    /// </summary>
    public static TokenListParser<SqlToken, T> OneOf<T>(params (SqlToken Kind, T Value)[] alternatives)
    {
        var map = alternatives.ToDictionary(a => a.Kind, a => a.Value);
        var expectations = alternatives.Select(a => a.Kind.ToString().ToLowerInvariant()).ToArray();
        return input =>
        {
            var next = input.ConsumeToken();
            return next.HasValue && map.TryGetValue(next.Value.Kind, out var value)
                ? TokenListParserResult.Value(value, input, next.Remainder)
                : TokenListParserResult.Empty<SqlToken, T>(input, expectations);
        };
    }

    /// <summary><c>'(' p ')'</c></summary>
    public static TokenListParser<SqlToken, T> Parens<T>(TokenListParser<SqlToken, T> parser) =>
        from _ in LParen
        from value in parser
        from __ in RParen
        select value;

    // ------------------------------------------------------------ punctuation and operators

    public static readonly TokenListParser<SqlToken, Token<SqlToken>> LParen = Lex.Kind(SqlToken.LParen);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> RParen = Lex.Kind(SqlToken.RParen);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Comma = Lex.Kind(SqlToken.Comma);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Dot = Lex.Kind(SqlToken.Dot);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Star = Lex.Kind(SqlToken.Star);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Semicolon = Lex.Kind(SqlToken.Semicolon);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Plus = Lex.Kind(SqlToken.Plus);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Minus = Lex.Kind(SqlToken.Minus);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Slash = Lex.Kind(SqlToken.Slash);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Concat = Lex.Kind(SqlToken.Concat);

    // ------------------------------------------------------------ tokens with values

    // IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords)
    public static readonly TokenListParser<SqlToken, string> Identifier =
        Lex.Kind(SqlToken.Identifier).Select(t => t.ToStringValue());

    public static readonly TokenListParser<SqlToken, string> QuotedIdentifier =
        Lex.Kind(SqlToken.QuotedIdentifier).Select(t => Unquote(t, '"'));

    public static readonly TokenListParser<SqlToken, string> String =
        Lex.Kind(SqlToken.String).Select(t => Unquote(t, '\''));

    public static readonly TokenListParser<SqlToken, string> Number =
        Lex.Kind(SqlToken.Number).Select(t => t.ToStringValue());

    // INTEGER ::= [0-9]+   (a NUMBER token without fraction or exponent)
    public static readonly TokenListParser<SqlToken, int> Integer = Lex.Kind(SqlToken.Number)
        .Where(t => int.TryParse(Chars(t), NumberStyles.None, CultureInfo.InvariantCulture, out _), "integer")
        .Select(t => int.Parse(Chars(t), NumberStyles.None, CultureInfo.InvariantCulture));

    private static ReadOnlySpan<char> Chars(Token<SqlToken> token) =>
        token.Span.Source.AsSpan(token.Span.Position.Absolute, token.Span.Length);

    private static string Unquote(Token<SqlToken> token, char quote)
    {
        var text = token.ToStringValue();
        return text.Substring(1, text.Length - 2).Replace(new string(quote, 2), quote.ToString());
    }
}
