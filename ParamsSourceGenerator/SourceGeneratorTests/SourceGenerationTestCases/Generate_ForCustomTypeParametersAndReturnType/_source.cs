using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    public class InnerClass
    {
    }

    [Params(MaxOverrides = 1)]
    private static InnerClass Format(InnerClass format, ReadOnlySpan<InnerClass> args)
    {
        return format;
    }
}
