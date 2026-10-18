using System.Diagnostics.CodeAnalysis;
using Pidgin;

namespace SqlParsers.Pidgin;

/// <summary>
/// A named grammar rule whose definition is assigned later, so rules can be written in any order and refer
/// to each other recursively. Pidgin's <c>Rec</c> does the same through a <see cref="Lazy{T}"/>; this is a
/// plain forward reference that costs one virtual call.
/// </summary>
/// <remarks>Overrides <see cref="Parser{TToken,T}.TryParse"/>, which Pidgin documents as an unstable extension point.</remarks>
internal sealed class Rule<T>(string name) : Parser<char, T>
{
    private Parser<char, T>? _definition;

    public string Name { get; } = name;

    public Parser<char, T> Definition
    {
        get => _definition ?? throw new InvalidOperationException($"Rule '{Name}' has no definition.");
        set => _definition = value;
    }

    public override bool TryParse(
        ref ParseState<char> state,
        ref PooledList<Expected<char>> expecteds,
        [MaybeNullWhen(false)] out T result) =>
        _definition!.TryParse(ref state, ref expecteds, out result);
}
