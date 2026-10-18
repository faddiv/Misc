using Superpower;
using Superpower.Model;

namespace SqlParsers.Superpower;

/// <summary>
/// The tokens of <c>grammar/sql92-select.ebnf</c>, recognized by a hand-written <see cref="Tokenizer{TKind}"/>.
/// <para>
/// This replaced a <c>TokenizerBuilder</c> with regular expressions. The builder tries its recognizers one after
/// another at every position (about 20 here, several of them regexes), which took half of the parse time. This
/// tokenizer picks the token by its first character, and it looks each word up once: a keyword becomes its own
/// token kind, so the parser compares kinds instead of comparing the text of every word with every keyword it tries.
/// </para>
/// </summary>
internal sealed class SqlTokenizer : Tokenizer<SqlToken>
{
    private static readonly Dictionary<string, SqlToken>.AlternateLookup<ReadOnlySpan<char>> Keywords =
        SqlKeywords.Reserved
            .ToDictionary(k => k, k => Enum.Parse<SqlToken>(k, ignoreCase: true), StringComparer.OrdinalIgnoreCase)
            .GetAlternateLookup<ReadOnlySpan<char>>();

    protected override IEnumerable<Result<SqlToken>> Tokenize(TextSpan span)
    {
        var next = SkipTrivia(span);
        while (next.HasValue)
        {
            var start = next.Location;
            var c = next.Value;
            next = next.Remainder.ConsumeChar();
            SqlToken kind;

            if (IsIdentifierStart(c))
            {
                // IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (a keyword if it is a reserved word)
                next = SkipWhile(next, IsIdentifierPart);
                kind = Keywords.TryGetValue(Text(start, next.Location), out var keyword) ? keyword : SqlToken.Identifier;
            }
            else if (char.IsAsciiDigit(c) || (c == '.' && next.HasValue && char.IsAsciiDigit(next.Value)))
            {
                // NUMBER ::= ( [0-9]+ ('.' [0-9]*)? | '.' [0-9]+ ) ( [eE] [+-]? [0-9]+ )?
                next = SkipWhile(next, char.IsAsciiDigit);
                if (c != '.' && next.HasValue && next.Value == '.')
                    next = SkipWhile(next.Remainder.ConsumeChar(), char.IsAsciiDigit);
                if (next.HasValue && next.Value is 'e' or 'E')
                {
                    var exponent = next.Remainder.ConsumeChar();
                    if (exponent.HasValue && exponent.Value is '+' or '-') exponent = exponent.Remainder.ConsumeChar();
                    if (exponent.HasValue && char.IsAsciiDigit(exponent.Value)) next = SkipWhile(exponent, char.IsAsciiDigit);
                }
                if (next.HasValue && IsIdentifierPart(next.Value))
                {
                    yield return Result.Empty<SqlToken>(next.Location, "a delimiter after the number");
                    yield break;
                }
                kind = SqlToken.Number;
            }
            else if (c is '\'' or '"')
            {
                // STRING ::= "'" ( [^'] | "''" )* "'"      QUOTED_IDENTIFIER ::= '"' ( [^"] | '""' )+ '"'
                var empty = true;
                while (true)
                {
                    if (!next.HasValue)
                    {
                        yield return Result.Empty<SqlToken>(next.Location, $"closing {c}");
                        yield break;
                    }
                    if (next.Value == c)
                    {
                        var after = next.Remainder.ConsumeChar();
                        if (!after.HasValue || after.Value != c) { next = after; break; }   // closing quote
                        next = after;                                                        // doubled quote
                    }
                    next = next.Remainder.ConsumeChar();
                    empty = false;
                }
                if (c == '"' && empty)
                {
                    yield return Result.Empty<SqlToken>(start, "a non-empty quoted identifier");
                    yield break;
                }
                kind = c == '"' ? SqlToken.QuotedIdentifier : SqlToken.String;
            }
            else
            {
                // Operators and punctuation; two-character operators first.
                var second = next.HasValue ? next.Value : '\0';
                switch (c)
                {
                    case '<' when second == '>': kind = SqlToken.NotEqual; next = next.Remainder.ConsumeChar(); break;
                    case '<' when second == '=': kind = SqlToken.LessThanOrEqual; next = next.Remainder.ConsumeChar(); break;
                    case '>' when second == '=': kind = SqlToken.GreaterThanOrEqual; next = next.Remainder.ConsumeChar(); break;
                    case '|' when second == '|': kind = SqlToken.Concat; next = next.Remainder.ConsumeChar(); break;
                    case '=': kind = SqlToken.Equal; break;
                    case '<': kind = SqlToken.LessThan; break;
                    case '>': kind = SqlToken.GreaterThan; break;
                    case '(': kind = SqlToken.LParen; break;
                    case ')': kind = SqlToken.RParen; break;
                    case ',': kind = SqlToken.Comma; break;
                    case ';': kind = SqlToken.Semicolon; break;
                    case '*': kind = SqlToken.Star; break;
                    case '+': kind = SqlToken.Plus; break;
                    case '-': kind = SqlToken.Minus; break;
                    case '/': kind = SqlToken.Slash; break;
                    case '.': kind = SqlToken.Dot; break;
                    default:
                        yield return Result.Empty<SqlToken>(start);
                        yield break;
                }
            }

            yield return Result.Value(kind, start, next.Location);
            next = SkipTrivia(next.Location);
        }
    }

    private static bool IsIdentifierStart(char c) => char.IsAsciiLetter(c) || c == '_';

    private static bool IsIdentifierPart(char c) => char.IsAsciiLetterOrDigit(c) || c == '_';

    private static Result<char> SkipWhile(Result<char> next, Func<char, bool> predicate)
    {
        while (next.HasValue && predicate(next.Value)) next = next.Remainder.ConsumeChar();
        return next;
    }

    // Whitespace and line comments:  ( [ \t\r\n]+ | '--' up to the end of the line )*
    private static Result<char> SkipTrivia(TextSpan span)
    {
        var next = span.ConsumeChar();
        while (next.HasValue)
        {
            if (char.IsWhiteSpace(next.Value))
            {
                next = next.Remainder.ConsumeChar();
            }
            else if (next.Value == '-' && next.Remainder.ConsumeChar() is { HasValue: true, Value: '-' })
            {
                next = SkipWhile(next.Remainder.ConsumeChar(), c => c is not ('\r' or '\n'));
            }
            else
            {
                break;
            }
        }
        return next;
    }

    private static ReadOnlySpan<char> Text(TextSpan start, TextSpan end) =>
        start.Source.AsSpan(start.Position.Absolute, end.Position.Absolute - start.Position.Absolute);
}
