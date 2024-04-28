// <auto-generated />

#nullable enable

namespace Something
{
    partial class Foo
    {
        public static void Format<T>(string format, T args0)
        {
            var foxyParamsArray = new Arguments1<T>(args0);
            Format<T>(format, global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref foxyParamsArray.arg0, 1));
        }

        public static void Format<T>(string format, T args0, T args1)
        {
            var foxyParamsArray = new Arguments2<T>(args0, args1);
            Format<T>(format, global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref foxyParamsArray.arg0, 2));
        }

        public static void Format<T>(string format, params T[] args)
        {
            Format<T>(format, new global::System.ReadOnlySpan<T>(args));
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