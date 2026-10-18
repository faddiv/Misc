using Superpower;
using Superpower.Model;

namespace SqlParsers.Superpower;

/// <summary>
/// Allocation-free versions of Superpower combinators that the grammar uses on hot paths.
/// <para>
/// They have the same names and signatures as the library's and are declared in this namespace, so the
/// compiler picks them before the ones imported with <c>using Superpower</c>, and the grammar (LINQ queries
/// included) reads exactly as it would with the library's versions. The behaviour is the same (see <see cref="IsPartial{T}"/> for the one
/// difference); what changes is
/// that nothing is allocated per parse except the results:
/// </para>
/// <list type="bullet">
/// <item><c>SelectMany</c> (every <c>from</c> after the first in a LINQ query): Superpower's is
/// <c>parser.Then(t => selector(t).Select(u => projector(t, u)))</c>, which creates a parser and two closures
/// each time it runs.</item>
/// <item><c>AtLeastOnceDelimitedBy</c> (every qualified name and list): Superpower's builds
/// <c>delimiter.IgnoreThen(parser).Many()</c> anew for every match, then copies the list twice.</item>
/// <item><c>Many</c>: Superpower's allocates a list even when nothing matches, which is the usual case for
/// the join and set-operator tails.</item>
/// </list>
/// </summary>
internal static class Combinators
{
    public static TokenListParser<SqlToken, V> SelectMany<T, U, V>(
        this TokenListParser<SqlToken, T> parser,
        Func<T, TokenListParser<SqlToken, U>> selector,
        Func<T, U, V> projector) =>
        input =>
        {
            var first = parser(input);
            if (!first.HasValue)
                return TokenListParserResult.CastEmpty<SqlToken, T, V>(first);

            var second = selector(first.Value)(first.Remainder);
            if (!second.HasValue)
                return TokenListParserResult.CastEmpty<SqlToken, U, V>(second);

            return TokenListParserResult.Value(projector(first.Value, second.Value), input, second.Remainder);
        };

    public static TokenListParser<SqlToken, T[]> Many<T>(this TokenListParser<SqlToken, T> parser) =>
        input =>
        {
            var remainder = input;
            var result = parser(input);
            if (!result.HasValue)
                return IsPartial(result, remainder)
                    ? TokenListParserResult.CastEmpty<SqlToken, T, T[]>(result)
                    : TokenListParserResult.Value(Array.Empty<T>(), input, input);

            var items = new List<T>();
            while (result.HasValue)
            {
                if (result.Remainder == remainder)
                    throw new InvalidOperationException("Many() cannot be applied to zero-width parsers.");
                items.Add(result.Value);
                remainder = result.Remainder;
                result = parser(remainder);
            }
            return IsPartial(result, remainder)
                ? TokenListParserResult.CastEmpty<SqlToken, T, T[]>(result)
                : TokenListParserResult.Value(items.ToArray(), input, remainder);
        };

    public static TokenListParser<SqlToken, T[]> AtLeastOnceDelimitedBy<T, U>(
        this TokenListParser<SqlToken, T> parser,
        TokenListParser<SqlToken, U> delimiter) =>
        input =>
        {
            var first = parser(input);
            if (!first.HasValue)
                return TokenListParserResult.CastEmpty<SqlToken, T, T[]>(first);

            var remainder = first.Remainder;
            var separator = delimiter(remainder);
            if (!separator.HasValue)
                return IsPartial(separator, remainder)
                    ? TokenListParserResult.CastEmpty<SqlToken, U, T[]>(separator)
                    : TokenListParserResult.Value(new[] { first.Value }, input, remainder);

            var items = new List<T> { first.Value };
            while (separator.HasValue)
            {
                var item = parser(separator.Remainder);
                if (!item.HasValue)
                    return TokenListParserResult.CastEmpty<SqlToken, T, T[]>(item);
                items.Add(item.Value);
                remainder = item.Remainder;
                separator = delimiter(remainder);
            }
            return !separator.HasValue && IsPartial(separator, remainder)
                ? TokenListParserResult.CastEmpty<SqlToken, U, T[]>(separator)
                : TokenListParserResult.Value(items.ToArray(), input, remainder);
        };

    /// <summary>
    /// A failure that consumed input is an error, not "no match". (Superpower also lets a failure marked by
    /// <c>Try()</c> back out here; that flag is internal, and the grammar only uses <c>Try()</c> around a whole
    /// alternative of <c>Or</c>, never directly inside these combinators.)
    /// </summary>
    private static bool IsPartial<T>(TokenListParserResult<SqlToken, T> result, TokenList<SqlToken> from) =>
        result.SubTokenErrorPosition.HasValue || result.Remainder != from;
}
