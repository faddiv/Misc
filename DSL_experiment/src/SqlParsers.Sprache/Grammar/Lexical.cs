using System.Globalization;
using Sprache;

namespace SqlParsers.Sprache;

/// <summary>
/// Lexical rules of <c>grammar/sql92-select.ebnf</c>. Sprache is scannerless, so every token parser here also
/// skips the whitespace and comments that follow it (see <see cref="Token{T}"/>). Sprache's <c>Or</c> always
/// backtracks, so no <c>Try</c> is needed anywhere. Keywords, identifiers, punctuation and trivia are primitive
/// parsers (see <c>PrimitiveParsers.cs</c>).
/// <para>
/// In a LINQ query only the first <c>from</c> source is evaluated once; later sources are evaluated on every parse.
/// Combinators used there are therefore built beforehand, into the fields below.
/// </para>
/// </summary>
internal static class Lex
{
    // Whitespace and line comments between tokens:  ( [ \t\r\n]+ | '--' up to the end of the line )*
    public static readonly Parser<string> Skip = input => Result.Success(string.Empty, PrimitiveParsers.SkipTrivia(input));

    /// <summary>A token: <paramref name="parser"/> followed by any whitespace or comments.</summary>
    public static Parser<T> Token<T>(Parser<T> parser) => PrimitiveParsers.Token(parser);

    /// <summary>A punctuation or operator token.</summary>
    public static Parser<string> Symbol(string text) => PrimitiveParsers.Symbol(text);

    /// <summary>A case-insensitive keyword. The whole word must match, so SELECT does not match SELECTED.</summary>
    public static Parser<string> Keyword(string keyword) => PrimitiveParsers.Keyword(keyword);

    /// <summary><c>'(' p ')'</c></summary>
    public static Parser<T> Parens<T>(Parser<T> parser) =>
        from _ in LParen
        from value in parser
        from __ in RParen
        select value;

    // ------------------------------------------------------------ punctuation

    public static readonly Parser<string> LParen = Symbol("(");
    public static readonly Parser<string> RParen = Symbol(")");
    public static readonly Parser<string> Comma = Symbol(",");
    public static readonly Parser<string> Dot = Symbol(".");
    public static readonly Parser<string> Star = Symbol("*");
    public static readonly Parser<string> Semicolon = Symbol(";");

    // ------------------------------------------------------------ tokens

    // IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords)
    public static readonly Parser<string> Identifier = PrimitiveParsers.Identifier();

    // QUOTED_IDENTIFIER ::= '"' ( [^"] | '""' )+ '"'
    private static readonly Parser<string> QuotedIdentifierBody =
        Parse.CharExcept('"').Or(Parse.String("\"\"").Return('"')).AtLeastOnce().Text();
    private static readonly Parser<char> DoubleQuote = Parse.Char('"');
    public static readonly Parser<string> QuotedIdentifier = Token(
        from _ in DoubleQuote
        from body in QuotedIdentifierBody
        from __ in DoubleQuote
        select body);

    // STRING ::= "'" ( [^'] | "''" )* "'"
    private static readonly Parser<string> StringBody =
        Parse.CharExcept('\'').Or(Parse.String("''").Return('\'')).Many().Text();
    private static readonly Parser<char> SingleQuote = Parse.Char('\'');
    public static readonly Parser<string> String = Token(
        from _ in SingleQuote
        from body in StringBody
        from __ in SingleQuote
        select body);

    // NUMBER ::= ( [0-9]+ ('.' [0-9]*)? | '.' [0-9]+ ) ( [eE] [+-]? [0-9]+ )?   -- kept as written
    private static readonly Parser<string> Digits = Parse.Digit.AtLeastOnce().Text();
    private static readonly Parser<string> OptionalDigits = Parse.Digit.Many().Text();
    private static readonly Parser<char> DecimalPoint = Parse.Char('.');
    private static readonly Parser<IOption<char>> OptionalSign = Parse.Chars('+', '-').Optional();

    private static readonly Parser<string> Fraction =                         // '.' [0-9]*
        from _ in DecimalPoint
        from digits in OptionalDigits
        select "." + digits;
    private static readonly Parser<IOption<string>> OptionalFraction = Fraction.Optional();

    private static readonly Parser<string> Exponent =                         // [eE] [+-]? [0-9]+
        from e in Parse.Chars('e', 'E')
        from sign in OptionalSign
        from digits in Digits
        select e + (sign.IsDefined ? sign.Get().ToString() : string.Empty) + digits;
    private static readonly Parser<IOption<string>> OptionalExponent = Exponent.Optional();

    private static readonly Parser<string> Mantissa =
        (from integer in Digits                                                // [0-9]+ ('.' [0-9]*)?
         from fraction in OptionalFraction
         select integer + fraction.GetOrElse(string.Empty))
        .Or(from _ in DecimalPoint                                             // '.' [0-9]+
            from digits in Digits
            select "." + digits);

    public static readonly Parser<string> Number = Token(
        from mantissa in Mantissa
        from exponent in OptionalExponent
        select mantissa + exponent.GetOrElse(string.Empty));

    // INTEGER ::= [0-9]+
    public static readonly Parser<int> Integer = Token(Digits).Select(s => int.Parse(s, CultureInfo.InvariantCulture));
}
