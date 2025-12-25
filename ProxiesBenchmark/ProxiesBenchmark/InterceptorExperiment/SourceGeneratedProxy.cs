using System;
using System.Reflection;
using ExperimentalInterceptor;

namespace ProxiesBenchmark.InterceptorExperiment
{
    public class SourceGeneratedProxy : ICalculator
    {
        private readonly IInterceptor interceptor;
        private static readonly MethodInfo _addMethod = typeof(ICalculator).GetMethod(nameof(Add));
        private static readonly MethodInfo _throwMethod = typeof(ICalculator).GetMethod(nameof(Throw));
        private static readonly Func<ICalculator, int, int, int> _addFunc = (target, a, b) => target.Add(a, b);

        private static readonly Func<ICalculator, string, object> _throwAction = (target, message) =>
        {
            target.Throw(message);
            return null;
        };

        public ICalculator Target { get; }

        public SourceGeneratedProxy(ICalculator target, IInterceptor interceptor)
        {
            this.interceptor = interceptor;
            Target = target;
        }

        public int Add(int a, int b)
        {
            var context = ProxyGenerator.CreateContext(Target, a, b, _addFunc, _addMethod);
            return interceptor.Invoke<InterceptorContext2<ICalculator, int, int, int>, int>(context);
        }

        public void Throw(string message)
        {
            var context = ProxyGenerator.CreateContext(Target, message, _throwAction, _throwMethod);
            interceptor.Invoke<InterceptorContext1<ICalculator, string, object>, object>(context);
        }
    }
}
