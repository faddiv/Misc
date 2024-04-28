using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 2)]
    private static string Format(string format, ReadOnlySpan<string> args)
    {
        return format;
    }
}
