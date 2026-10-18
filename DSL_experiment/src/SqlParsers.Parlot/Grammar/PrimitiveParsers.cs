using Parlot;
using Parlot.Fluent;
using Parlot.Rewriting;

namespace SqlParsers.Parlot;

/// <summary>
/// Keywords, identifiers and the whitespace/comments between tokens, written as primitive parsers that read
/// straight from the scanner's buffer.
/// <para>
/// The combinator versions did more work than needed. A keyword was <c>Literals.Identifier().When(...)</c>: it
/// scanned the whole word and called a lambda to compare it, for every keyword tried at a position. The trivia
/// rule needed a <c>Deferred</c> (for "comment, then more trivia") and ran three parsers after every token.
/// </para>
/// <para>
/// Keywords and identifiers implement <see cref="ISeekable"/> (the characters they can start with), like Parlot's
/// own literals. <c>OneOf</c> uses that to jump straight to the alternatives that can match the next character.
/// </para>
/// </summary>
internal static class PrimitiveParsers
{
    public static readonly char[] IdentifierStartChars =
        [.. Enumerable.Range('A', 26).Select(c => (char)c), .. Enumerable.Range('a', 26).Select(c => (char)c), '_'];

    public static bool IsIdentifierStart(char c) => char.IsAsciiLetter(c) || c == '_';

    public static bool IsIdentifierPart(char c) => char.IsAsciiLetterOrDigit(c) || c == '_';

    /// <summary>Length of the word ( [A-Za-z_] [A-Za-z0-9_]* ) at <paramref name="offset"/>; 0 if none.</summary>
    public static int WordLength(string buffer, int offset)
    {
        if (offset >= buffer.Length || !IsIdentifierStart(buffer[offset])) return 0;
        var end = offset + 1;
        while (end < buffer.Length && IsIdentifierPart(buffer[end])) end++;
        return end - offset;
    }
}

/// <summary>A case-insensitive keyword. The whole word must match, so SELECT does not match SELECTED.</summary>
internal sealed class KeywordParser(string keyword) : Parser<TextSpan>, ISeekable
{
    public bool CanSeek => true;

    public char[] ExpectedChars { get; } = [char.ToUpperInvariant(keyword[0]), char.ToLowerInvariant(keyword[0])];

    public bool SkipWhitespace => false;

    public override bool Parse(ParseContext context, ref ParseResult<TextSpan> result)
    {
        context.EnterParser(this);
        var buffer = context.Scanner.Buffer;
        var start = context.Scanner.Cursor.Offset;
        var end = start + keyword.Length;
        if (end <= buffer.Length
            && buffer.AsSpan(start, keyword.Length).Equals(keyword, StringComparison.OrdinalIgnoreCase)
            && (end == buffer.Length || !PrimitiveParsers.IsIdentifierPart(buffer[end])))
        {
            context.Scanner.Cursor.AdvanceNoNewLines(keyword.Length);
            result.Set(start, end, new TextSpan(buffer, start, keyword.Length));
            context.ExitParser(this);
            return true;
        }
        context.ExitParser(this);
        return false;
    }
}

/// <summary>IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords)</summary>
internal sealed class IdentifierParser : Parser<string>, ISeekable
{
    public bool CanSeek => true;

    public char[] ExpectedChars => PrimitiveParsers.IdentifierStartChars;

    public bool SkipWhitespace => false;

    public override bool Parse(ParseContext context, ref ParseResult<string> result)
    {
        context.EnterParser(this);
        var buffer = context.Scanner.Buffer;
        var start = context.Scanner.Cursor.Offset;
        var length = PrimitiveParsers.WordLength(buffer, start);
        if (length == 0 || SqlKeywords.IsReserved(buffer.AsSpan(start, length)))
        {
            context.ExitParser(this);
            return false;
        }
        context.Scanner.Cursor.AdvanceNoNewLines(length);
        result.Set(start, start + length, buffer.Substring(start, length));
        context.ExitParser(this);
        return true;
    }
}

/// <summary>( [ \t\r\n]+ | '--' up to the end of the line )*   -- never fails</summary>
internal sealed class TriviaParser : Parser<bool>
{
    public override bool Parse(ParseContext context, ref ParseResult<bool> result)
    {
        context.EnterParser(this);
        var scanner = context.Scanner;
        var buffer = scanner.Buffer;
        var start = scanner.Cursor.Offset;
        while (true)
        {
            scanner.SkipWhiteSpaceOrNewLine();
            var offset = scanner.Cursor.Offset;
            if (offset + 1 >= buffer.Length || buffer[offset] != '-' || buffer[offset + 1] != '-') break;
            var end = offset + 2;
            while (end < buffer.Length && buffer[end] is not ('\r' or '\n')) end++;
            scanner.Cursor.AdvanceNoNewLines(end - offset);
        }
        result.Set(start, scanner.Cursor.Offset, true);
        context.ExitParser(this);
        return true;
    }
}
