using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 2)]
    private static void Format<T>(string format, ReadOnlySpan<T> args)
    {
    }
}
