using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 2)]
    private void Format(string args3, ReadOnlySpan<object> args)
    {
    }
}
