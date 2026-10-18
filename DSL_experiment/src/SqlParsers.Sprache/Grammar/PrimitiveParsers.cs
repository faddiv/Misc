using Sprache;

namespace SqlParsers.Sprache;

/// <summary>
/// The hottest lexical rules written as primitive parsers: keywords, identifiers, punctuation and the
/// whitespace/comments between tokens. They read straight from <see cref="IInput.Source"/>, so a keyword that
/// does not match costs a few character comparisons.
/// <para>
/// The combinator versions allocated on every attempt: <c>Parse.IgnoreCase(k)</c> is a chain of
/// <c>Concat</c>/<c>Once</c> parsers that builds enumerables, every failing <c>Parse.Char</c> formats an
/// "unexpected 'x'" message, and the token rule <c>from value in p from _ in Skip select value</c> ran a
/// <c>Many</c> and a LINQ step after every token. The identifier was built from <c>Letter</c> + <c>Many().Text()</c>
/// and then rejected with <c>Where</c> if it was a keyword.
/// </para>
/// <para>A failed primitive consumes nothing and returns a failure with a fixed message.</para>
/// </summary>
internal static class PrimitiveParsers
{
    public static bool IsIdentifierStart(char c) => char.IsAsciiLetter(c) || c == '_';

    public static bool IsIdentifierPart(char c) => char.IsAsciiLetterOrDigit(c) || c == '_';

    /// <summary><paramref name="parser"/> followed by any whitespace or comments.</summary>
    public static Parser<T> Token<T>(Parser<T> parser) =>
        input =>
        {
            var result = parser(input);
            return result.WasSuccessful ? Result.Success(result.Value, SkipTrivia(result.Remainder)) : result;
        };

    /// <summary>A case-insensitive keyword (the whole word), followed by trivia.</summary>
    public static Parser<string> Keyword(string keyword)
    {
        string[] expectations = [keyword];
        return input =>
        {
            var source = input.Source.AsSpan(input.Position);
            if (source.Length >= keyword.Length
                && source[..keyword.Length].Equals(keyword, StringComparison.OrdinalIgnoreCase)
                && (source.Length == keyword.Length || !IsIdentifierPart(source[keyword.Length])))
            {
                return Result.Success(keyword, SkipTrivia(Advance(input, keyword.Length)));
            }
            return Result.Failure<string>(input, "unexpected word", expectations);
        };
    }

    /// <summary>IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords), followed by trivia.</summary>
    public static Parser<string> Identifier()
    {
        string[] expectations = ["identifier"];
        return input =>
        {
            var source = input.Source.AsSpan(input.Position);
            var length = 0;
            if (!source.IsEmpty && IsIdentifierStart(source[0]))
            {
                length = 1;
                while (length < source.Length && IsIdentifierPart(source[length])) length++;
            }
            if (length == 0 || SqlKeywords.IsReserved(source[..length]))
                return Result.Failure<string>(input, "unexpected input", expectations);
            return Result.Success(source[..length].ToString(), SkipTrivia(Advance(input, length)));
        };
    }

    /// <summary>A punctuation or operator token, followed by trivia.</summary>
    public static Parser<string> Symbol(string text)
    {
        string[] expectations = [text];
        return input =>
            input.Source.AsSpan(input.Position).StartsWith(text, StringComparison.Ordinal)
                ? Result.Success(text, SkipTrivia(Advance(input, text.Length)))
                : Result.Failure<string>(input, "unexpected input", expectations);
    }

    /// <summary>( [ \t\r\n]+ | '--' up to the end of the line )*</summary>
    public static IInput SkipTrivia(IInput input)
    {
        var source = input.Source;
        var position = input.Position;
        while (position < source.Length)
        {
            var c = source[position];
            if (char.IsWhiteSpace(c))
            {
                position++;
            }
            else if (c == '-' && position + 1 < source.Length && source[position + 1] == '-')
            {
                position += 2;
                while (position < source.Length && source[position] is not ('\r' or '\n')) position++;
            }
            else
            {
                break;
            }
        }
        return Advance(input, position - input.Position);
    }

    private static IInput Advance(IInput input, int count)
    {
        for (var i = 0; i < count; i++) input = input.Advance();
        return input;
    }
}
