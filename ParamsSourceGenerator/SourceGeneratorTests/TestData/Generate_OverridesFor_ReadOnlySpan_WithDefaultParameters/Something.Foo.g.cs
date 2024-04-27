// <auto-generated />

#nullable enable

namespace Something
{
    partial class Foo
    {
        public static void Format(string format, object args0)
        {
            var foxyParamsArray = new Arguments1<object>(args0);
            Format(format, global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref foxyParamsArray.arg0, 1));
        }

        public static void Format(string format, object args0, object args1)
        {
            var foxyParamsArray = new Arguments2<object>(args0, args1);
            Format(format, global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref foxyParamsArray.arg0, 2));
        }

        public static void Format(string format, object args0, object args1, object args2)
        {
            var foxyParamsArray = new Arguments3<object>(args0, args1, args2);
            Format(format, global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref foxyParamsArray.arg0, 3));
        }

        public static void Format(string format, params object[] args)
        {
            Format(format, new global::System.ReadOnlySpan<object>(args));
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

    [global::System.Runtime.CompilerServices.InlineArray(3)]
    file struct Arguments3<T>
    {
        public T arg0;
        public Arguments3(T value0, T value1, T value2)
        {
            arg0 = value0;
            this[1] = value1;
            this[2] = value2;
        }
    }
}
