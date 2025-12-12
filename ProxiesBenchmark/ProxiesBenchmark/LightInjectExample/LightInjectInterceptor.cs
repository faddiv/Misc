using System;
using System.Reflection;
using LightInject.Interception;

namespace ProxiesBenchmark.LightInjectExample
{
    internal class LightInjectInterceptor : IInterceptor
    {
        private ulong callCount = 0;
        private ulong errorCount = 0;
        private int lastResult = 0;
        private ValueTuple<int, int> lastInput = (0, 0);

        public ulong CallCount => callCount;

        public ulong ErrorCount => errorCount;

        public object LastResult => lastResult;

        public object LastInput => lastInput;

        public object Invoke(IInvocationInfo invocationInfo)
        {
            try
            {
                callCount++;
                if (invocationInfo.Arguments.Length == 2)
                {
                    lastInput = ((int)invocationInfo.Arguments[0], (int)invocationInfo.Arguments[1]);
                }

                var proceed = invocationInfo.Proceed();
                lastResult = (int)proceed;
                return proceed;
            }
            catch (TargetInvocationException ex)
            {
                errorCount++;
                throw new Exception($"Error in target method {invocationInfo.Method.Name}", ex.InnerException);
            }
        }
    }
}
