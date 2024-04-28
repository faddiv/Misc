using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 1)]
    private static void Format(in string format1, out string format2, ref string format3, ref readonly string format4, ReadOnlySpan<object> args)
    {
        format2 = default;
    }
}
