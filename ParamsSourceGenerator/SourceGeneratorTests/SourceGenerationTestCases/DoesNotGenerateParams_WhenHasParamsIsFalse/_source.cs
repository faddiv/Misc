using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 1, HasParams = false)]
    private static void Format(string format, ReadOnlySpan<object> args)
    {
    }
}
