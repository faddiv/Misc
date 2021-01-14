using System;
using System.Runtime.CompilerServices;

namespace ProxiesBenchmark
{
    public class CalculatorProxy : ICalculator
    {
        public string Msg { get; set; }
        public CalculatorProxy(ICalculator target)
        {
            Target = target;
        }

        public ICalculator Target { get; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int Add(int a, int b)
        {
            return Target.Add(a, b);
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
                Msg = ex.Message;
                throw;
            }
        }
    }
}
