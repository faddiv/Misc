using Sprache;

namespace SqlParsers.Sprache;

/// <summary>
/// An <see cref="IInput"/> that creates each position once per parse.
/// <para>
/// Sprache's own <see cref="Input"/> allocates a new input object and a new memo dictionary for every character
/// it advances over, and a backtracking parser advances over the same characters many times. This one keeps the
/// positions of the current parse in an array, so advancing to a position reached before returns the same
/// object, and it creates the memo dictionary only if someone asks for it (this grammar never does).
/// </para>
/// </summary>
internal sealed class SourceInput : IInput
{
    private readonly SourceInput?[] _positions;
    private IDictionary<object, object>? _memos;

    private SourceInput(string source, int position, int line, int column, SourceInput?[] positions)
    {
        Source = source;
        Position = position;
        Line = line;
        Column = column;
        _positions = positions;
    }

    public static IInput Start(string source)
    {
        var positions = new SourceInput?[source.Length + 1];
        return positions[0] = new SourceInput(source, 0, 1, 1, positions);
    }

    public string Source { get; }

    public int Position { get; }

    public int Line { get; }

    public int Column { get; }

    public char Current => Source[Position];

    public bool AtEnd => Position == Source.Length;

    public IDictionary<object, object> Memos => _memos ??= new Dictionary<object, object>();

    public IInput Advance()
    {
        if (AtEnd) throw new InvalidOperationException("The input is already at the end of the source.");
        var newLine = Current == '\n';
        return _positions[Position + 1] ??= new SourceInput(
            Source, Position + 1, newLine ? Line + 1 : Line, newLine ? 1 : Column + 1, _positions);
    }

    public bool Equals(IInput? other) =>
        other is not null && (ReferenceEquals(this, other) || (Position == other.Position && string.Equals(Source, other.Source)));

    public override bool Equals(object? obj) => Equals(obj as IInput);

    public override int GetHashCode() => (Source.GetHashCode() * 397) ^ Position;

    public override string ToString() => $"Line {Line}, Column {Column}";
}
