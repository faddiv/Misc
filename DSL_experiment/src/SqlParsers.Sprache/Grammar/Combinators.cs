using Sprache;

namespace SqlParsers.Sprache;

/// <summary>
/// Allocation-free versions of the Sprache combinators that run most often.
/// <para>
/// They have the same names and signatures as the library's and are declared in this namespace, so the
/// compiler picks them before the ones imported with <c>using Sprache</c>, and the grammar (LINQ queries
/// included) reads exactly as it would with the library's versions. The behaviour is the same; what changes is
/// that they allocate only their results. Sprache builds new parsers while parsing:
/// </para>
/// <list type="bullet">
/// <item><c>SelectMany</c> (every <c>from</c> after the first in a LINQ query) is
/// <c>parser.Then(t => selector(t).Select(u => projector(t, u)))</c>.</item>
/// <item><c>Select</c> and <c>Return(value)</c> are <c>parser.Then(t => Parse.Return(convert(t)))</c>.</item>
/// <item><c>Then</c> creates a closure for every successful first parser, <c>Or</c> one for every failed one.</item>
/// <item><c>ChainOperator</c> (every binary-operator level of an expression) builds the parser for the rest of
/// the chain after every operand.</item>
/// <item><c>DelimitedBy</c> is a LINQ query over <c>Once</c> and <c>Repeat</c> that is rebuilt on every match.</item>
/// <item><c>Many</c> and <c>Optional</c> allocate an empty list or a <c>None</c> when nothing matches.</item>
/// </list>
/// </summary>
internal static class Combinators
{
    public static Parser<V> SelectMany<T, U, V>(this Parser<T> parser, Func<T, Parser<U>> selector, Func<T, U, V> projector) =>
        input =>
        {
            var first = parser(input);
            if (!first.WasSuccessful)
                return Result.Failure<V>(first.Remainder, first.Message, first.Expectations);

            var second = selector(first.Value)(first.Remainder);
            return second.WasSuccessful
                ? Result.Success(projector(first.Value, second.Value), second.Remainder)
                : Result.Failure<V>(second.Remainder, second.Message, second.Expectations);
        };

    public static Parser<U> Then<T, U>(this Parser<T> first, Func<T, Parser<U>> second) =>
        input =>
        {
            var result = first(input);
            return result.WasSuccessful
                ? second(result.Value)(result.Remainder)
                : Result.Failure<U>(result.Remainder, result.Message, result.Expectations);
        };

    public static Parser<U> Select<T, U>(this Parser<T> parser, Func<T, U> convert) =>
        input =>
        {
            var result = parser(input);
            return result.WasSuccessful
                ? Result.Success(convert(result.Value), result.Remainder)
                : Result.Failure<U>(result.Remainder, result.Message, result.Expectations);
        };

    public static Parser<U> Return<T, U>(this Parser<T> parser, U value) =>
        input =>
        {
            var result = parser(input);
            return result.WasSuccessful
                ? Result.Success(value, result.Remainder)
                : Result.Failure<U>(result.Remainder, result.Message, result.Expectations);
        };

    public static Parser<T> Or<T>(this Parser<T> first, Parser<T> second) =>
        input =>
        {
            var firstResult = first(input);
            if (firstResult.WasSuccessful && !firstResult.Remainder.Equals(input))
                return firstResult;

            var secondResult = second(input);
            if (secondResult.WasSuccessful)
                return secondResult;
            return firstResult.WasSuccessful ? firstResult : BestError(firstResult, secondResult);
        };

    /// <summary>The failure that got further; at the same position, the expectations of both.</summary>
    private static IResult<T> BestError<T>(IResult<T> first, IResult<T> second)
    {
        if (second.Remainder.Position > first.Remainder.Position) return second;
        if (second.Remainder.Position < first.Remainder.Position) return first;
        return Result.Failure<T>(first.Remainder, first.Message, first.Expectations.Union(second.Expectations));
    }

    public static Parser<IEnumerable<T>> Many<T>(this Parser<T> parser) =>
        input =>
        {
            var result = parser(input);
            if (!result.WasSuccessful || result.Remainder.Equals(input))
                return Result.Success<IEnumerable<T>>(Array.Empty<T>(), input);

            var items = new List<T>();
            var remainder = input;
            while (result.WasSuccessful && !remainder.Equals(result.Remainder))
            {
                items.Add(result.Value);
                remainder = result.Remainder;
                result = parser(remainder);
            }
            return Result.Success<IEnumerable<T>>(items, remainder);
        };

    public static Parser<IEnumerable<T>> AtLeastOnce<T>(this Parser<T> parser)
    {
        var many = parser.Many();
        return input =>
        {
            var first = parser(input);
            if (!first.WasSuccessful)
                return Result.Failure<IEnumerable<T>>(first.Remainder, first.Message, first.Expectations);
            var rest = many(first.Remainder);
            return Result.Success(Prepend(first.Value, rest.Value), rest.Remainder);
        };
    }

    /// <summary><c>parser (delimiter parser)*</c>; a delimiter not followed by an item is left unconsumed.</summary>
    public static Parser<IEnumerable<T>> DelimitedBy<T, U>(this Parser<T> parser, Parser<U> delimiter) =>
        input =>
        {
            var first = parser(input);
            if (!first.WasSuccessful)
                return Result.Failure<IEnumerable<T>>(first.Remainder, first.Message, first.Expectations);

            List<T>? items = null;
            var remainder = first.Remainder;
            while (true)
            {
                var separator = delimiter(remainder);
                if (!separator.WasSuccessful) break;
                var item = parser(separator.Remainder);
                if (!item.WasSuccessful) break;
                (items ??= [first.Value]).Add(item.Value);
                remainder = item.Remainder;
            }
            return Result.Success<IEnumerable<T>>(items ?? (IEnumerable<T>)[first.Value], remainder);
        };

    public static Parser<IOption<T>> Optional<T>(this Parser<T> parser) =>
        input =>
        {
            var result = parser(input);
            return result.WasSuccessful
                ? Result.Success<IOption<T>>(new Option<T>(result.Value), result.Remainder)
                : Result.Success<IOption<T>>(Option<T>.None, input);
        };

    /// <summary><c>operand (op operand)*</c>, folded to the left with <paramref name="apply"/>.</summary>
    public static Parser<T> ChainOperator<T, TOp>(Parser<TOp> op, Parser<T> operand, Func<TOp, T, T, T> apply) =>
        input =>
        {
            var first = operand(input);
            if (!first.WasSuccessful)
                return first;

            var value = first.Value;
            var remainder = first.Remainder;
            while (true)
            {
                var opResult = op(remainder);
                if (!opResult.WasSuccessful) break;
                var next = operand(opResult.Remainder);
                if (!next.WasSuccessful) break;   // like Sprache's (built on Or): an operator without operand backs out
                value = apply(opResult.Value, value, next.Value);
                remainder = next.Remainder;
            }
            return Result.Success(value, remainder);
        };

    private static IEnumerable<T> Prepend<T>(T first, IEnumerable<T> rest) =>
        rest is T[] { Length: 0 } ? [first] : rest is List<T> list ? PrependList(first, list) : rest.Prepend(first);

    private static List<T> PrependList<T>(T first, List<T> list)
    {
        list.Insert(0, first);
        return list;
    }

    private sealed class Option<T> : IOption<T>
    {
        public static readonly Option<T> None = new();

        private readonly T _value = default!;

        private Option() => IsEmpty = true;

        public Option(T value) => _value = value;

        public bool IsEmpty { get; }

        public bool IsDefined => !IsEmpty;

        public T GetOrDefault() => _value;

        public T Get() => IsEmpty ? throw new InvalidOperationException("Cannot get value from None.") : _value;
    }
}
