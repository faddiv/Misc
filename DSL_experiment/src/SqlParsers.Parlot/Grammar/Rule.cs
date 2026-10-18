using Parlot;
using Parlot.Fluent;

namespace SqlParsers.Parlot;

/// <summary>
/// A named grammar rule whose definition is assigned later, so rules can be written in any order and refer
/// to each other recursively.
/// <para>
/// Parlot's <see cref="Deferred{T}"/> serves the same purpose but also performs cycle detection and recursion
/// tracking on every call, which made the parser 1.4-1.7x slower when every rule used it. This grammar has no
/// left recursion, so a plain forward reference is enough and costs one virtual call.
/// </para>
/// </summary>
internal sealed class Rule<T> : Parser<T>
{
    private Parser<T>? _definition;

    public Parser<T> Definition
    {
        get => _definition ?? throw new InvalidOperationException($"Rule '{Name}' has no definition.");
        set => _definition = value;
    }

    public override bool Parse(ParseContext context, ref ParseResult<T> result) => _definition!.Parse(context, ref result);
}
