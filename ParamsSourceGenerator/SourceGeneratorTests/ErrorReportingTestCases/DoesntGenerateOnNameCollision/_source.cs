using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 1)]
    private static void Format(string values, ReadOnlySpan<object> {|#0:values|})
    {
    }
}
