using System.Globalization;
using Pidgin;
using static Pidgin.Parser;

namespace SqlParsers.Pidgin;

/// <summary>
/// Lexical rules of <c>grammar/sql92-select.ebnf</c>. Pidgin is scannerless, so every token parser here also
/// skips the whitespace and comments that follow it (see <see cref="Token{T}"/>).
/// <para>
/// Pidgin does not backtrack after consuming input, so token parsers that can fail part-way (multi-character
/// operators, the escaped quotes) are wrapped in <c>Try</c>. A failed token then consumes nothing. Keywords, identifiers
/// and trivia are primitive parsers (<see cref="KeywordParser"/>, <see cref="IdentifierParser"/>,
/// <see cref="TriviaParser"/>), see <c>PrimitiveParsers.cs</c>.
/// </para>
/// </summary>
internal static class Lex
{
    // Static fields initialize in textual order: Skip must stay first.

    // Whitespace and line comments between tokens:  ( [ \t\r\n]+ | '--' up to the end of the line )*
    public static readonly Parser<char, Unit> Skip = new TriviaParser();

    /// <summary>A token: <paramref name="parser"/> followed by any whitespace or comments.</summary>
    public static Parser<char, T> Token<T>(Parser<char, T> parser) => parser.Before(Skip);

    public static Parser<char, char> Symbol(char c) => Token(Char(c));

    public static Parser<char, string> Operator(string text) => Token(Try(Parser.String(text)));

    /// <summary>A case-insensitive keyword. The whole word must match, so SELECT does not match SELECTED.</summary>
    public static Parser<char, Unit> Keyword(string keyword) => Token(new KeywordParser(keyword));

    /// <summary><c>'(' p ')'</c></summary>
    public static Parser<char, T> Parens<T>(Parser<char, T> parser) => LParen.Then(parser).Before(RParen);

    // ------------------------------------------------------------ punctuation

    public static readonly Parser<char, char> LParen = Symbol('(');
    public static readonly Parser<char, char> RParen = Symbol(')');
    public static readonly Parser<char, char> Comma = Symbol(',');
    public static readonly Parser<char, char> Dot = Symbol('.');
    public static readonly Parser<char, char> Star = Symbol('*');
    public static readonly Parser<char, char> Semicolon = Symbol(';');

    // ------------------------------------------------------------ tokens

    // IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords)
    public static readonly Parser<char, string> Identifier = Token(new IdentifierParser());

    // QUOTED_IDENTIFIER ::= '"' ( [^"] | '""' )+ '"'
    public static readonly Parser<char, string> QuotedIdentifier = Token(
        Char('"')
            .Then(Parser<char>.Token(c => c != '"').Or(Try(Parser.String("\"\"")).ThenReturn('"')).AtLeastOnceString())
            .Before(Char('"')));

    // STRING ::= "'" ( [^'] | "''" )* "'"
    public static readonly Parser<char, string> String = Token(
        Char('\'')
            .Then(Parser<char>.Token(c => c != '\'').Or(Try(Parser.String("''")).ThenReturn('\'')).ManyString())
            .Before(Char('\'')));

    // NUMBER ::= ( [0-9]+ ('.' [0-9]*)? | '.' [0-9]+ ) ( [eE] [+-]? [0-9]+ )?   -- kept as written
    public static readonly Parser<char, string> Number = CreateNumber();

    // INTEGER ::= [0-9]+
    public static readonly Parser<char, int> Integer =
        Token(Digit.AtLeastOnceString()).Select(s => int.Parse(s, CultureInfo.InvariantCulture));

    private static Parser<char, string> CreateNumber()
    {
        var digits = Digit.AtLeastOnceString();
        var none = Parser<char>.Return(string.Empty);
        var sign = Char('+').Or(Char('-')).Select(c => c.ToString()).Or(none);
        var exponent = Try(Map((e, s, d) => e + s + d,                          // [eE] [+-]? [0-9]+
            Char('e').Or(Char('E')).Select(c => c.ToString()), sign, digits));
        var mantissa = Map((i, f) => i + f,                                      // [0-9]+ ('.' [0-9]*)?
                digits, Char('.').Then(Digit.ManyString(), (p, d) => p + d).Or(none))
            .Or(Try(Char('.').Then(digits, (p, d) => p + d)));                   // '.' [0-9]+
        return Token(Map((m, e) => m + e, mantissa, exponent.Or(none)));
    }
}
