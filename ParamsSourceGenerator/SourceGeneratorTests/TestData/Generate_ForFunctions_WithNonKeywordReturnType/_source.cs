using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 2)]
    private Foo Format(string format, ReadOnlySpan<string> args)
    {
        return this;
    }
}
