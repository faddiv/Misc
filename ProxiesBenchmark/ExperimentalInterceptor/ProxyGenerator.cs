using System;
using System.Reflection;

namespace ExperimentalInterceptor
{
    public static class ProxyGenerator
    {
        public static InterceptorContext0<TTarget, TResult> CreateContext<TTarget, TResult>(
            TTarget target,
            Func<TTarget, TResult> func,
            MethodInfo method)
        {
            return new InterceptorContext0<TTarget, TResult>(target, func, method);
        }

        public static InterceptorContext1<TTarget, T1, TResult> CreateContext<TTarget, T1, TResult>(
            TTarget target,
            T1 arg1,
            Func<TTarget, T1, TResult> func,
            MethodInfo method)
        {
            return new InterceptorContext1<TTarget, T1, TResult>(target, arg1, func, method);
        }

        public static InterceptorContext2<TTarget, T1, T2, TResult> CreateContext<TTarget, T1, T2, TResult>(
            TTarget target,
            T1 arg1,
            T2 arg2,
            Func<TTarget, T1, T2, TResult> func,
            MethodInfo method)
        {
            return new InterceptorContext2<TTarget, T1, T2, TResult>(target, arg1, arg2, func, method);
        }
    }
}
