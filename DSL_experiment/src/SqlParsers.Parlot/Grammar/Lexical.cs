using System.Globalization;
using Parlot;
using Parlot.Fluent;

namespace SqlParsers.Parlot;

/// <summary>
/// Lexical rules of <c>grammar/sql92-select.ebnf</c>. Parlot is scannerless, so every token parser here
/// also skips the whitespace and comments that follow it (see <see cref="Token{T}"/>).
/// </summary>
internal static class Lex
{
    // Static fields initialize in textual order: Skip and Word must stay first.

    // Whitespace and line comments between tokens:  ( [ \t\r\n]+ | '--' up to the end of the line )*
    // Written with ZeroOrOne and recursion instead of ZeroOrMany, so skipping never allocates.
    public static readonly Parser<bool> Skip = CreateSkip();

    private static readonly Parser<TextSpan> Word = Literals.Identifier();

    /// <summary>A token: <paramref name="parser"/> followed by any whitespace or comments.</summary>
    public static Parser<T> Token<T>(Parser<T> parser) => parser.AndSkip(Skip);

    public static Parser<char> Symbol(char c) => Token(Literals.Char(c));

    public static Parser<string> Operator(string text) => Token(Literals.Text(text));

    /// <summary>A case-insensitive keyword. The whole word must match, so SELECT does not match SELECTED.</summary>
    public static Parser<TextSpan> Keyword(string keyword) =>
        Token(Word.When((_, w) => w.Span.Equals(keyword, StringComparison.OrdinalIgnoreCase)));

    /// <summary><c>'(' p ')'</c></summary>
    public static Parser<T> Parens<T>(Parser<T> parser) => LParen.SkipAnd(parser).AndSkip(RParen);

    // ------------------------------------------------------------ punctuation

    public static readonly Parser<char> LParen = Symbol('(');
    public static readonly Parser<char> RParen = Symbol(')');
    public static readonly Parser<char> Comma = Symbol(',');
    public static readonly Parser<char> Dot = Symbol('.');
    public static readonly Parser<char> Star = Symbol('*');
    public static readonly Parser<char> Semicolon = Symbol(';');

    // ------------------------------------------------------------ tokens

    // IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords)
    public static readonly Parser<string> Identifier =
        Token(Word.When(static (_, w) => !SqlKeywords.IsReserved(w.Span)))
            .Then(static w => w.ToString());

    // QUOTED_IDENTIFIER ::= '"' ( [^"] | '""' )+ '"'
    public static readonly Parser<string> QuotedIdentifier =
        Token(Literals.Char('"')
                .SkipAnd(OneOrMany(OneOf(
                    Literals.Pattern(static c => c != '"').Then(static s => s.ToString()),
                    Literals.Text("\"\"").Then(static _ => "\""))))
                .AndSkip(Literals.Char('"')))
            .Then(static parts => string.Concat(parts));

    // STRING ::= "'" ( [^'] | "''" )* "'"
    public static readonly Parser<string> String =
        Token(Literals.Char('\'')
                .SkipAnd(ZeroOrMany(OneOf(
                    Literals.Pattern(static c => c != '\'').Then(static s => s.ToString()),
                    Literals.Text("''").Then(static _ => "'"))))
                .AndSkip(Literals.Char('\'')))
            .Then(static parts => string.Concat(parts));

    // NUMBER ::= ( [0-9]+ ('.' [0-9]*)? | '.' [0-9]+ ) ( [eE] [+-]? [0-9]+ )?   -- kept as written
    public static readonly Parser<string> Number = CreateNumber();

    // INTEGER ::= [0-9]+
    public static readonly Parser<int> Integer =
        Token(Literals.Pattern(static c => char.IsAsciiDigit(c)))
            .Then(static d => int.Parse(d.Span, NumberStyles.None, CultureInfo.InvariantCulture));

    // ------------------------------------------------------------ helpers

    private static Parser<bool> CreateSkip()
    {
        // skip ::= whitespace? ('--' rest-of-line skip)?
        var skip = Deferred<bool>();
        var commentThenSkip = Literals.Text("--")
            .SkipAnd(ZeroOrOne(Literals.Pattern(static c => c != '\r' && c != '\n')))
            .SkipAnd(skip);
        skip.Parser = ZeroOrOne(Literals.WhiteSpace(includeNewLines: true))
            .SkipAnd(ZeroOrOne(commentThenSkip))
            .Then(static _ => true);
        return skip;
    }

    private static Parser<string> CreateNumber()
    {
        var digits = Literals.Pattern(static c => char.IsAsciiDigit(c));
        var fraction = Literals.Char('.')                               // '.' [0-9]*
            .SkipAnd(ZeroOrOne(digits.Then(static d => d.ToString())))
            .Then(static d => "." + d);
        var exponent = Literals.Pattern(static c => c is 'e' or 'E', maxSize: 1)   // [eE] [+-]? [0-9]+
            .And(ZeroOrOne(Literals.Pattern(static c => c is '+' or '-', maxSize: 1).Then(static s => s.ToString())))
            .And(digits)
            .Then(static t => t.Item1 + t.Item2 + t.Item3);
        var mantissa = OneOf(
            digits.And(ZeroOrOne(fraction)).Then(static t => t.Item1.ToString() + t.Item2),   // [0-9]+ ('.' [0-9]*)?
            Literals.Char('.').SkipAnd(digits).Then(static d => "." + d.ToString()));          // '.' [0-9]+
        return Token(mantissa.And(ZeroOrOne(exponent)).Then(static t => t.Item1 + t.Item2));
    }
}
