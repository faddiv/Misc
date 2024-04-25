using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 1)]
    private static void Format<T, F, G, H>(string format, ReadOnlySpan<object> args)
            where T : struct
            where F : class, ICloneable, new()
            where G : notnull, Attribute
            where H : unmanaged
    {
    }
}
