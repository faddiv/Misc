using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 2)]
    private static T Format<T>(string format, ReadOnlySpan<object> args)
    {
        return default(T);
    }
}
