using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace ProxiesBenchmark.CastleProxy
{
    public class CastleExampleInterceptor : IInterceptor
    {
        private ulong callCount = 0;
        private ulong errorCount = 0;
        private int lastResult = 0;
        private ValueTuple<int, int> lastInput = (0, 0);

        public ulong CallCount => callCount;

        public ulong ErrorCount => errorCount;

        public object LastResult => lastResult;

        public object LastInput => lastInput;

        public void Intercept(IInvocation invocation)
        {
            try
            {
                callCount++;
                if (invocation.Arguments.Length == 2)
                {
                    lastInput = ((int)invocation.Arguments[0], (int)invocation.Arguments[1]);
                }

                invocation.Proceed();
                lastResult = (int)invocation.ReturnValue;
            }
            catch (TargetInvocationException ex)
            {
                errorCount++;
                throw new Exception($"Error in target method {invocation.Method.Name}", ex.InnerException);
            }
        }
    }
}
