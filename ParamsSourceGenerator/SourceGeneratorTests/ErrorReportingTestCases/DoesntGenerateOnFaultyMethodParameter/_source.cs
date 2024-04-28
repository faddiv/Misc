using Foxy.Params;
using System;

namespace Something;

public class Foo
{
    [Params]
    private static void Format({|#0:ThisIsWrong|} format, ReadOnlySpan<object> args)
    {
    }
}
