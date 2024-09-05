using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PartialStringComparer;

public static class InterpolatedComparer
{
    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Used by InterpolatedComparisonHandler")]
    public static bool Equals(
        string? stringToCompare,
        [InterpolatedStringHandlerArgument("stringToCompare")] InterpolatedComparisonHandler right)
    {
        return right.GetResult();
    }

#pragma warning disable CS9113 // Parameter is unread.
    [InterpolatedStringHandler]
    public ref struct InterpolatedComparisonHandler(int literalLength, int formattedCount, string? stringToCompare)
    {
        private int _index = 0;
        private readonly ReadOnlySpan<char> _stringToCompare = stringToCompare.AsSpan();
        private bool _isEqual = stringToCompare != null && stringToCompare.Length >= literalLength;

        public void AppendLiteral(string? s)
        {
            if (s is null)
            {
                return;
            }

            AppendFormatted(s.AsSpan());
        }

        public void AppendFormatted(string? t)
        {
            if (t is null)
            {
                return;
            }

            AppendFormatted(t.AsSpan());
        }

        public void AppendFormatted(ReadOnlySpan<char> t)
        {
            if (IsNextSegmentEqual(t))
            {
                _index += t.Length;
            }
            else
            {
                _isEqual = false;
            }
        }

        public void AppendFormatted<T>(T? t)
        {
            if (ShouldSkip(t))
            {
                return;
            }

            if (t is IFormattable formattable)
            {
                if (formattable is ISpanFormattable spanFormattable)
                {
                    Span<char> space = stackalloc char[64];

                    if (spanFormattable.TryFormat(space, out var charsWritten, ReadOnlySpan<char>.Empty, null))
                    {
                        if (charsWritten > space.Length)
                        {
                            throw new FormatException($"One of the parameter has a faulty ISpanFormattable implementation.");
                        }

                        var result = (ReadOnlySpan<char>)space[0..charsWritten];
                        if (IsNextSegmentEqual(result))
                        {
                            _index += result.Length;
                        }
                        else
                        {
                            _isEqual = false;
                        }

                        return;
                    }
                }
                var formattedValue = formattable.ToString(null, null);
                AppendFormatted(formattedValue);
            }
            else
            {
                AppendFormatted(t.ToString());
            }
        }

        internal readonly bool GetResult() => _isEqual && _index == _stringToCompare.Length;

        private readonly bool IsNextSegmentEqual(ReadOnlySpan<char> s)
        {
            if (_isEqual == false)
            {
                return false;
            }

            if (s.Length == 0)
            {
                return true;
            }

            if (_index + s.Length > _stringToCompare.Length)
            {
                return false;
            }

            var span = _stringToCompare[_index..];

            if (span.StartsWith(s))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private readonly bool ShouldSkip<T>([NotNullWhen(false)] T? t)
        {
            if (!_isEqual)
            {
                return true;
            }

            if (t is null)
            {
                return true;
            }

            return false;
        }
    }
#pragma warning restore CS9113 // Parameter is unread.
}
