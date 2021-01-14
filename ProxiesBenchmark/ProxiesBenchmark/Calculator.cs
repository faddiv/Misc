using System;
using System.Runtime.CompilerServices;

namespace ProxiesBenchmark
{
    public class Calculator : MarshalByRefObject, ICalculator
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
