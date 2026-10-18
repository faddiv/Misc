using Sprache;

namespace SqlParsers.Sprache;

/// <summary>
/// Named grammar rules whose definitions are assigned later, so rules can be written in any order and refer to
/// each other recursively.
/// <para>
/// Sprache parsers are delegates, so a rule is a delegate bound to a small forwarding object;
/// <see cref="Define{T}"/> finds that object through <see cref="Delegate.Target"/>. A rule is therefore an ordinary
/// <see cref="Parser{T}"/>: LINQ queries and combinators work on it directly. Like <c>Parse.Ref</c>, calling it costs
/// one extra delegate invocation.
/// </para>
/// </summary>
internal static class Rule
{
    public static Parser<T> Create<T>(string name) => new Forward<T>(name).Parse;

    public static void Define<T>(this Parser<T> rule, Parser<T> definition)
    {
        if (rule.Target is not Forward<T> forward)
            throw new ArgumentException("Not a rule created by Rule.Create.", nameof(rule));
        forward.Definition = definition;
    }

    private sealed class Forward<T>(string name)
    {
        public Parser<T>? Definition;

        public IResult<T> Parse(IInput input) =>
            (Definition ?? throw new InvalidOperationException($"Rule '{name}' has no definition."))(input);

        public override string ToString() => name;
    }
}
