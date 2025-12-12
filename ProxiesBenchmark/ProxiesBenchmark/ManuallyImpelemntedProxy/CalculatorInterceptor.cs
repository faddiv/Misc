using System;
using System.Runtime.CompilerServices;

namespace ProxiesBenchmark.ManuallyImpelemntedProxy
{
    public class CalculatorInterceptor : ICalculator
    {
        private ulong callCount = 0;
        private ulong errorCount = 0;
        private int lastResult = 0;
        private ValueTuple<int, int> lastInput = (0, 0);

        public string Msg { get; set; }

        public CalculatorInterceptor(ICalculator target)
        {
            Target = target;
        }

        public ICalculator Target { get; }

        public ulong CallCount => callCount;

        public ulong ErrorCount => errorCount;

        public object LastResult => lastResult;

        public object LastInput => lastInput;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int Add(int a, int b)
        {
            callCount++;
            lastInput = (a, b);
            var add = Target.Add(a, b);
            lastResult = add;
            return add;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Throw(string message)
        {
            try
            {
                Target.Throw(message);
            }
            catch (Exception ex)
            {
                errorCount++;
                Msg = ex.Message;
                throw;
            }
        }
    }
}
