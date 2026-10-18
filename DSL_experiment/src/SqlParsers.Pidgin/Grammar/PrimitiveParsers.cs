using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Pidgin;

namespace SqlParsers.Pidgin;

/// <summary>
/// The hottest lexical rules written as primitive parsers: keywords, identifiers and the whitespace/comments
/// between tokens. They read straight from the input span, so a keyword that does not match costs a few
/// character comparisons.
/// <para>
/// The combinator versions were the hottest code in the parser: <c>Try(CIString(k).Then(Not(IdentifierPart)))</c>
/// takes a bookmark and lower-cases every character through the culture tables, and it runs for every keyword
/// alternative tried at a position (a primary tries NULL, CASE, CAST and EXISTS before the identifier). The
/// identifier was built from <c>Letter</c> + <c>ManyString</c> (two strings) and then rejected if it was a keyword.
/// </para>
/// <para>
/// Trivia skipping ran after every token and, as <c>(Whitespace.SkipAtLeastOnce() | Try("--" ...)).SkipMany()</c>,
/// recorded two failed alternatives each time even when there was nothing to skip.
/// </para>
/// <para>
/// The keyword and identifier parsers consume nothing when they fail, so they need no <c>Try</c>. They override
/// <see cref="Parser{TToken,T}.TryParse"/>, which Pidgin documents as an unstable extension point.
/// </para>
/// </summary>
internal static class PrimitiveParsers
{
    public static bool IsIdentifierStart(char c) => char.IsAsciiLetter(c) || c == '_';

    public static bool IsIdentifierPart(char c) => char.IsAsciiLetterOrDigit(c) || c == '_';

    /// <summary>Length of the word ( [A-Za-z_] [A-Za-z0-9_]* ) at the start of <paramref name="input"/>; 0 if none.</summary>
    public static int WordLength(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty || !IsIdentifierStart(input[0])) return 0;
        var i = 1;
        while (i < input.Length && IsIdentifierPart(input[i])) i++;
        return i;
    }

    internal static void Fail(ref ParseState<char> state, ref PooledList<Expected<char>> expecteds, Expected<char> expected)
    {
        var eof = !state.HasCurrent;
        state.SetError(eof ? Maybe.Nothing<char>() : Maybe.Just(state.Current), eof, state.Location, null);
        expecteds.Add(expected);
    }
}

/// <summary>A case-insensitive keyword. The whole word must match, so SELECT does not match SELECTED.</summary>
internal sealed class KeywordParser(string keyword) : Parser<char, Unit>
{
    private readonly Expected<char> _expected = new([.. keyword]);

    public override bool TryParse(ref ParseState<char> state, ref PooledList<Expected<char>> expecteds, out Unit result)
    {
        result = Unit.Value;
        // One character more than the keyword, to check that the word ends there.
        var span = state.LookAhead(keyword.Length + 1);
        if (span.Length >= keyword.Length
            && span[..keyword.Length].Equals(keyword, StringComparison.OrdinalIgnoreCase)
            && (span.Length == keyword.Length || !PrimitiveParsers.IsIdentifierPart(span[keyword.Length])))
        {
            state.Advance(keyword.Length);
            return true;
        }
        PrimitiveParsers.Fail(ref state, ref expecteds, _expected);
        return false;
    }
}

/// <summary>IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords)</summary>
internal sealed class IdentifierParser : Parser<char, string>
{
    private static readonly Expected<char> Expected = new("identifier");

    public override bool TryParse(
        ref ParseState<char> state,
        ref PooledList<Expected<char>> expecteds,
        [MaybeNullWhen(false)] out string result)
    {
        // Look ahead in growing chunks until the word ends (identifiers are usually short).
        var chunk = 32;
        var span = state.LookAhead(chunk);
        var length = PrimitiveParsers.WordLength(span);
        while (length == span.Length && span.Length == chunk)
        {
            chunk *= 4;
            span = state.LookAhead(chunk);
            length = PrimitiveParsers.WordLength(span);
        }

        if (length == 0 || SqlKeywords.IsReserved(span[..length]))
        {
            PrimitiveParsers.Fail(ref state, ref expecteds, Expected);
            result = null;
            return false;
        }
        result = span[..length].ToString();
        state.Advance(length);
        return true;
    }
}

/// <summary>( [ \t\r\n]+ | '--' up to the end of the line )*   -- never fails</summary>
internal sealed class TriviaParser : Parser<char, Unit>
{
    public override bool TryParse(ref ParseState<char> state, ref PooledList<Expected<char>> expecteds, out Unit result)
    {
        result = Unit.Value;
        while (state.HasCurrent)
        {
            var c = state.Current;
            if (char.IsWhiteSpace(c))
            {
                state.Advance();
            }
            else if (c == '-' && state.LookAhead(2) is ['-', '-'])
            {
                state.Advance(2);
                while (state.HasCurrent && state.Current is not ('\r' or '\n')) state.Advance();
            }
            else
            {
                break;
            }
        }
        return true;
    }
}
