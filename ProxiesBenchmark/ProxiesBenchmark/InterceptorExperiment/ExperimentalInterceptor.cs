using System;
using System.Reflection;
using ExperimentalInterceptor;

namespace ProxiesBenchmark.InterceptorExperiment
{
    public class ExperimentalInterceptor : IInterceptor
    {
        private ulong callCount = 0;
        private ulong errorCount = 0;
        private int lastResult = 0;
        private ValueTuple<int, int> lastInput = (0, 0);

        public ulong CallCount => callCount;

        public ulong ErrorCount => errorCount;

        public object LastResult => lastResult;

        public object LastInput => lastInput;

        public TResult Invoke<TContext, TResult>(TContext context) where TContext : IInterceptorContext<TResult>
        {
            try
            {
                callCount++;
                if (context.Length == 2)
                {
                    lastInput = (context.Arg<int>(0), context.Arg<int>(1));
                }

                var proceed = context.Invoke();
                lastResult = (int)(object)proceed;
                return proceed;
            }
            catch (TargetInvocationException ex)
            {
                errorCount++;
                throw new Exception($"Error in target method {context.Method.Name}", ex.InnerException);
            }
        }
    }
}
