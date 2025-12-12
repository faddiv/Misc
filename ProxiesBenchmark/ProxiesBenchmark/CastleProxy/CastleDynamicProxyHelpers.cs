using Castle.DynamicProxy;

namespace ProxiesBenchmark.CastleProxy
{
    public static class CastleDynamicProxyHelpers
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();


        public static T WithInheritedDynamicProxy<T>() where T : class
        {
            return _generator.CreateClassProxy<T>(new CastleExampleInterceptor());
        }

        public static T WithCompositeDynamicProxy<T>(T target) where T : class
        {
            return _generator.CreateInterfaceProxyWithTarget(target, new CastleExampleInterceptor());
        }
    }
}
