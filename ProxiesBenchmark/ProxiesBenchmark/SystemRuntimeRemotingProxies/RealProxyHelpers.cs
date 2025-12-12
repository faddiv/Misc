#if NET48
using System;

namespace ProxiesBenchmark.SystemRuntimeRemotingProxies
{
    public static class RealProxyHelpers
    {
        public static T WithRealProxy<T>(T target) where T : MarshalByRefObject
        {
            var decorator = new RealProxyExampleDecorator<T>(target);
            var proxy = decorator.GetTransparentProxy();
            return proxy as T;
        }
    }
}
#endif
