using System.Globalization;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace SqlParsers.Superpower;

public enum SqlToken
{
    Word,
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
}

/// <summary>
/// Lexical rules of <c>grammar/sql92-select.ebnf</c>. Superpower tokenizes first, so this class holds the
/// tokenizer and one parser per token. Keywords are <see cref="SqlToken.Word"/> tokens with a given text (see <see cref="K"/>).
/// </summary>
internal static class Lex
{
    // Recognizers are tried in order: multi-character operators before their prefixes, NUMBER before '.'.
    public static readonly Tokenizer<SqlToken> Tokenizer = new TokenizerBuilder<SqlToken>()
        .Ignore(Span.WhiteSpace)
        .Ignore(Span.Regex(@"--[^\r\n]*"))                                             // line comment
        .Match(Span.EqualTo("<>"), SqlToken.NotEqual)
        .Match(Span.EqualTo("<="), SqlToken.LessThanOrEqual)
        .Match(Span.EqualTo(">="), SqlToken.GreaterThanOrEqual)
        .Match(Span.EqualTo("||"), SqlToken.Concat)
        .Match(Character.EqualTo('='), SqlToken.Equal)
        .Match(Character.EqualTo('<'), SqlToken.LessThan)
        .Match(Character.EqualTo('>'), SqlToken.GreaterThan)
        .Match(Character.EqualTo('('), SqlToken.LParen)
        .Match(Character.EqualTo(')'), SqlToken.RParen)
        .Match(Character.EqualTo(','), SqlToken.Comma)
        .Match(Character.EqualTo(';'), SqlToken.Semicolon)
        .Match(Character.EqualTo('*'), SqlToken.Star)
        .Match(Character.EqualTo('+'), SqlToken.Plus)
        .Match(Character.EqualTo('-'), SqlToken.Minus)
        .Match(Character.EqualTo('/'), SqlToken.Slash)
        // NUMBER ::= ( [0-9]+ ('.' [0-9]*)? | '.' [0-9]+ ) ( [eE] [+-]? [0-9]+ )?
        .Match(Span.Regex(@"(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:[eE][+-]?[0-9]+)?"), SqlToken.Number, requireDelimiters: true)
        .Match(Character.EqualTo('.'), SqlToken.Dot)
        // STRING ::= "'" ( [^'] | "''" )* "'"
        .Match(Span.Regex("'(?:[^']|'')*'"), SqlToken.String)
        // QUOTED_IDENTIFIER ::= '"' ( [^"] | '""' )+ '"'
        .Match(Span.Regex("\"(?:[^\"]|\"\")+\""), SqlToken.QuotedIdentifier)
        // IDENTIFIER and keywords
        .Match(Span.Regex("[A-Za-z_][A-Za-z0-9_]*"), SqlToken.Word, requireDelimiters: true)
        .Build();

    /// <summary><c>'(' p ')'</c></summary>
    public static TokenListParser<SqlToken, T> Parens<T>(TokenListParser<SqlToken, T> parser) =>
        from _ in LParen
        from value in parser
        from __ in RParen
        select value;

    // ------------------------------------------------------------ punctuation and operators

    public static readonly TokenListParser<SqlToken, Token<SqlToken>> LParen = Token.EqualTo(SqlToken.LParen);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> RParen = Token.EqualTo(SqlToken.RParen);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Comma = Token.EqualTo(SqlToken.Comma);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Dot = Token.EqualTo(SqlToken.Dot);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Star = Token.EqualTo(SqlToken.Star);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Semicolon = Token.EqualTo(SqlToken.Semicolon);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Plus = Token.EqualTo(SqlToken.Plus);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Minus = Token.EqualTo(SqlToken.Minus);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Slash = Token.EqualTo(SqlToken.Slash);
    public static readonly TokenListParser<SqlToken, Token<SqlToken>> Concat = Token.EqualTo(SqlToken.Concat);

    // ------------------------------------------------------------ tokens with values

    // IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords)
    public static readonly TokenListParser<SqlToken, string> Identifier = Token.EqualTo(SqlToken.Word)
        .Where(t => !SqlKeywords.IsReserved(Chars(t)), "identifier")
        .Select(t => t.ToStringValue());

    public static readonly TokenListParser<SqlToken, string> QuotedIdentifier =
        Token.EqualTo(SqlToken.QuotedIdentifier).Select(t => Unquote(t, '"'));

    public static readonly TokenListParser<SqlToken, string> String =
        Token.EqualTo(SqlToken.String).Select(t => Unquote(t, '\''));

    public static readonly TokenListParser<SqlToken, string> Number =
        Token.EqualTo(SqlToken.Number).Select(t => t.ToStringValue());

    // INTEGER ::= [0-9]+   (a NUMBER token without fraction or exponent)
    public static readonly TokenListParser<SqlToken, int> Integer = Token.EqualTo(SqlToken.Number)
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
