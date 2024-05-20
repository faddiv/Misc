using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    public partial class Bar
    {
        [Params]
        private static void Format(string format, ReadOnlySpan<object> args)
        {
        }
    }
}
