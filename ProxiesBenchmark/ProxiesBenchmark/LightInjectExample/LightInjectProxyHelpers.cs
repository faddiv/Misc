using System;
using System.Linq.Expressions;
using LightInject.Interception;

namespace ProxiesBenchmark.LightInjectExample
{
    public class LightInjectProxyHelpers
    {
        private static Func<ICalculator> _createCalculator;
        private static readonly ProxyBuilder _proxyBuilder = new ProxyBuilder();
        private static Type _proxyType;


        public static void InitLightInject()
        {
            if (_createCalculator != null)
                return;
            _proxyType = _proxyBuilder.GetProxyType(new ProxyDefinition(typeof(Calculator), true).Implement(() => new LightInjectInterceptor()));
            var newExpression = Expression.New(_proxyType.GetConstructor(Type.EmptyTypes));
            _createCalculator = Expression.Lambda<Func<ICalculator>>(newExpression).Compile();
        }

        public static ICalculator WithLightInject()
        {
            return _createCalculator();
        }
    }
}
