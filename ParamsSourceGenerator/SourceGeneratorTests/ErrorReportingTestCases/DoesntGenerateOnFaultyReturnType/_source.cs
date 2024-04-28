using Foxy.Params;
using System;

namespace Something;

public class Foo
{
    [Params]
    private static {|#0:ThisIsWrong|} Format(string format, ReadOnlySpan<object> args)
    {
        return default;
    }
}
