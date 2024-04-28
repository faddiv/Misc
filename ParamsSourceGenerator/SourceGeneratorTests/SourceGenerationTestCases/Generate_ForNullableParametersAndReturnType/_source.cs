#nullable enable

using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 1)]
    private static string? Format(string? format, ReadOnlySpan<object?> args)
    {
        return format;
    }
}
