using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params]
    private static void Format(string format, ReadOnlySpan<object> args)
    {
    }
}
