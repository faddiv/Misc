using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [{|#0:Params|}]
    private static void Format(string format, out ReadOnlySpan<object> args)
    {
        args = new ReadOnlySpan<object>([new object()]);
    }
}
