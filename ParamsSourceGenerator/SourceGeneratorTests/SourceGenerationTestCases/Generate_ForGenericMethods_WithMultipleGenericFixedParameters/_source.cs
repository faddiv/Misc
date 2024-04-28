using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 2)]
    private static void Format<T, F>(T t, F f, ReadOnlySpan<object> args)
    {
    }
}
