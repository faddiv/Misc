using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [{|#0:Params(MaxOverrides = 3)|}]
    private static void Format(string valuesSpan, string values0, string values1, ReadOnlySpan<object> values)
    {
    }
}
