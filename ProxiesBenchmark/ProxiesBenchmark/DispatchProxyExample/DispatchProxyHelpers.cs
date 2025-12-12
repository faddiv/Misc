using System.Reflection;

namespace ProxiesBenchmark.DispatchProxyExample
{
    public static class DispatchProxyHelpers
    {
        public static T WithDispatchProxy<T>(object target) where T : class
        {
            var proxy = DispatchProxy.Create<T, DispatchProxyExampleInterceptor<T>>();
            if (proxy is DispatchProxyExampleInterceptor<T> decorator)
            {
                decorator.Target = (T)target;
            }

            return proxy;
        }
    }
}
