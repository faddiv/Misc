using System;
using System.Runtime.CompilerServices;

namespace ProxiesBenchmark
{
    public class Calculator : ICalculator
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual int Add(int a, int b)
        {
            return a + b;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual void Throw(string message)
        {
            throw new Exception(message);
        }
    }
    public class CalculatorMarshalled : MarshalByRefObject, ICalculator
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual int Add(int a, int b)
        {
            return a + b;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual void Throw(string message)
        {
            throw new Exception(message);
        }
    }
}
