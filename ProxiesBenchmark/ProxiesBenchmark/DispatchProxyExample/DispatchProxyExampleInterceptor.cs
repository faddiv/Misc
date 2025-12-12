using System;
using System.Reflection;

namespace ProxiesBenchmark.DispatchProxyExample
{
    public class DispatchProxyExampleInterceptor<T> : DispatchProxy where T : class
    {
        private ulong callCount = 0;
        private ulong errorCount = 0;
        private int lastResult = 0;
        private ValueTuple<int, int> lastInput = (0, 0);

        public ulong CallCount => callCount;

        public ulong ErrorCount => errorCount;

        public object LastResult => lastResult;

        public object LastInput => lastInput;

        public T Target { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                callCount++;
                if (args.Length == 2)
                {
                    lastInput = ((int)args[0], (int)args[1]);
                }

                var result = targetMethod.Invoke(Target, args);
                lastResult = (int)result;
                return result;
            }
            catch (TargetInvocationException exc)
            {
                errorCount++;
                throw new Exception($"Error in target method {targetMethod.Name}", exc.InnerException);
            }
        }
    }
}
