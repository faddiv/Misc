using System;
using System.Linq.Expressions;
using System.Reflection;
using Castle.DynamicProxy;
using LightInject.Interception;

namespace ProxiesBenchmark
{
    public static class Decorate
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();
        private static readonly ProxyBuilder _proxyBuilder = new ProxyBuilder();
        private static Type _proxyType;
        private static Func<ICalculator> _createCalculator;

        public static void InitLightInject()
        {
            if (_createCalculator != null)
                return;
            _proxyType = _proxyBuilder.GetProxyType(new ProxyDefinition(typeof(Calculator), true).Implement(() => new LightInjectInterceptor()));
            var newExpression = Expression.New(_proxyType.GetConstructor(Type.EmptyTypes));
            _createCalculator = Expression.Lambda<Func<ICalculator>>(newExpression).Compile();
        }
        public static T WithRealProxy<T>(T target) where T : MarshalByRefObject
        {
            var decorator = new RealProxyExampleDecorator<T>(target);
            var proxy = decorator.GetTransparentProxy();
            return proxy as T;
        }
        public static T WithDispatchProxy<T>(object target) where T : class
        {
            var proxy = DispatchProxy.Create<T, DispatchProxyExampleDecorator<T>>();
            (proxy as DispatchProxyExampleDecorator<T>).Target = (T)target;
            return proxy;
        }

        public static T WithInheritedDynamicProxy<T>() where T : class
        {
            return _generator.CreateClassProxy<T>(new CastleExampleInterceptor());
        }

        public static T WithCompositeDynamicProxy<T>(T target) where T : class
        {
            return _generator.CreateInterfaceProxyWithTarget<T>(target, new CastleExampleInterceptor());
        }

        public static ICalculator DecorateSimple(ICalculator calculator)
        {
            return new CalculatorProxy(calculator);
        }

        public static ICalculator WithLightInject()
        {
            return _createCalculator();
        }
    }
}
