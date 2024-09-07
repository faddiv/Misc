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

        public void AppendFormatted<T>(T t)
            where T : struct
        {
            if (ShouldSkip(t))
            {
                return;
            }

            if (t is IFormattable)
            {
                if (t is ISpanFormattable)
                {
                    Span<char> space = stackalloc char[64];
                    // When the t checked with is ISpanFormattable it becomes constrained abd calling with cast
                    // doesn't do boxing. The expection is enum hovewer it can be handled only with internal api. 
                    if (((ISpanFormattable)t).TryFormat(space, out var charsWritten, ReadOnlySpan<char>.Empty, null))
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

                var formattedValue = ((IFormattable)t).ToString(null, null);
                AppendFormatted(formattedValue);
            }
            else
            {
                AppendFormatted(t.ToString());
            }
        }

        public void AppendFormatted(object? t)
        {
            if (ShouldSkip(t))
            {
                return;
            }

            if (t is IFormattable formattable)
            {
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
