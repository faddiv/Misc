// <auto-generated />

#nullable enable

namespace Something
{
    partial class Foo
    {
        public static void Format(global::System.Exception ex, string format, object args0)
        {
            var foxyParamsArray = new Arguments1<object>(args0);
            Format(ex, format, global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref foxyParamsArray.arg0, 1));
        }

        public static void Format(global::System.Exception ex, string format, object args0, object args1)
        {
            var foxyParamsArray = new Arguments2<object>(args0, args1);
            Format(ex, format, global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref foxyParamsArray.arg0, 2));
        }

        public static void Format(global::System.Exception ex, string format, params object[] args)
        {
            Format(ex, format, new global::System.ReadOnlySpan<object>(args));
        }
    }

    [global::System.Runtime.CompilerServices.InlineArray(1)]
    file struct Arguments1<T>
    {
        public T arg0;

        public Arguments1(T value0)
        {
            arg0 = value0;
        }
    }

    [global::System.Runtime.CompilerServices.InlineArray(2)]
    file struct Arguments2<T>
    {
        public T arg0;

        public Arguments2(T value0, T value1)
        {
            arg0 = value0;
            this[1] = value1;
        }
    }
}
