using Foxy.Params;
using System;

namespace Something;

public partial class Foo
{
    [{|#0:Params|}]
    private static void Format(string format, {|#1:object args|})
    {
    }
}
