using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 1)]
    private static void Format<T>(string format, ReadOnlySpan<T> args)
            where T : class, ICloneable, new()
    {
    }
}
