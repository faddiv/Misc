using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 2)]
    private void Format(string format, ReadOnlySpan<object> args)
    {
    }
}
