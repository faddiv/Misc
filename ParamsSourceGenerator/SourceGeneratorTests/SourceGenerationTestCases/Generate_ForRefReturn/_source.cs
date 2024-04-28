using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 1)]
    private static ref string Format(ref string format, ReadOnlySpan<string> args)
    {
        return ref format;
    }
}
