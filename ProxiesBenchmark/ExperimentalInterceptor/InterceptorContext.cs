using System;
using System.Reflection;

namespace ExperimentalInterceptor
{
    public readonly struct InterceptorContext0<TTarget, TResult> : IInterceptorContext<TResult>
    {
        private readonly TTarget target;
        private readonly Func<TTarget, TResult> func;
        public object Target => target;
        public MethodInfo Method { get; }

        public int Length => 0;

        internal InterceptorContext0(TTarget target, Func<TTarget, TResult> func, MethodInfo method)
        {
            this.func = func;
            this.target = target;
            Method = method;
        }

        public T Arg<T>(int index)
        {
            throw new IndexOutOfRangeException();
        }

        public void Update<T>(int index, T value)
        {
            throw new IndexOutOfRangeException();
        }

        public TResult Invoke()
        {
            return func(target);
        }
    }

    public struct InterceptorContext1<TTarget, T1, TResult> : IInterceptorContext<TResult>
    {
        private readonly TTarget target;
        private T1 arg1;
        private readonly Func<TTarget, T1, TResult> func;
        public object Target => target;
        public MethodInfo Method { get; }

        public int Length => 1;

        internal InterceptorContext1(TTarget target, T1 arg1, Func<TTarget, T1, TResult> func, MethodInfo method)
        {
            this.arg1 = arg1;
            this.func = func;
            this.target = target;
            Method = method;
        }

        public T Arg<T>(int index)
        {
            if (index < 0 || index > 0) throw new IndexOutOfRangeException();
            if (arg1 is T t1)
            {
                return t1;
            }

            throw new InvalidCastException();
        }

        public void Update<T>(int index, T value)
        {
            if (index < 0 || index > 0) throw new IndexOutOfRangeException();
            if (value is T1 t1)
            {
                arg1 = t1;
            }

            else throw new InvalidCastException();
        }

        public TResult Invoke()
        {
            return func(target, arg1);
        }
    }

    public struct InterceptorContext2<TTarget, T1, T2, TResult> : IInterceptorContext<TResult>
    {
        private readonly TTarget target;
        private T1 arg1;
        private T2 arg2;
        private readonly Func<TTarget, T1, T2, TResult> func;
        public object Target => target;
        public MethodInfo Method { get; }

        public int Length => 1;

        internal InterceptorContext2(TTarget target, T1 arg1, T2 arg2, Func<TTarget, T1, T2, TResult> func, MethodInfo method)
        {
            this.func = func;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.target = target;
            Method = method;
        }

        public T Arg<T>(int index)
        {
            if (index < 0 || index > 1) throw new IndexOutOfRangeException();
            switch (index)
            {
                case 0 when arg1 is T t1:
                    return t1;
                case 1 when arg2 is T t2:
                    return t2;
                default:
                    throw new InvalidCastException();
            }
        }

        public void Update<T>(int index, T value)
        {
            if (index < 0 || index > 1) throw new IndexOutOfRangeException();
            switch (index)
            {
                case 0 when value is T1 t1:
                    arg1 = t1;
                    break;
                case 1 when value is T2 t2:
                    arg2 = t2;
                    break;
                default:
                    throw new InvalidCastException();
            }
        }

        public TResult Invoke()
        {
            return func(target, arg1, arg2);
        }
    }
}
