using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ProxiesBenchmark.CastleProxy;
using ProxiesBenchmark.DispatchProxyExample;
using ProxiesBenchmark.LightInjectExample;
using ProxiesBenchmark.ManuallyImpelemntedProxy;

namespace ProxiesBenchmark.Benchmarks
{
    [ArtifactsPath(".\\Benchmarks")]
    [MemoryDiagnoser]
    //[SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.Net80)]
    public class CreationBenchmarks : BenchmarksBase
    {
        private Calculator target;
#if NET48
        private CalculatorMarshalled target2;
#endif

        [GlobalSetup]
        public void Setup()
        {
            LightInjectProxyHelpers.InitLightInject();
            target = new Calculator();
#if NET48
            target2 = new CalculatorMarshalled();
#endif
        }

        [Benchmark]
        public ICalculator DecorateSimple()
        {
            return ManuallyImplementedProxyHelpers.DecorateSimple(target);
        }

#if NET48
        [Benchmark]
        public ICalculator WithRealProxy()
        {
            return ProxiesBenchmark.SystemRuntimeRemotingProxies.RealProxyHelpers.WithRealProxy(target2);
        }
#endif

        [Benchmark]
        public ICalculator WithDispatchProxy()
        {
            return DispatchProxyHelpers.WithDispatchProxy<ICalculator>(target);
        }

        [Benchmark]
        public ICalculator WithCompositeDynamicProxy()
        {
            return CastleDynamicProxyHelpers.WithCompositeDynamicProxy<ICalculator>(target);
        }

        [Benchmark]
        public ICalculator WithInheritedDynamicProxy()
        {
            return CastleDynamicProxyHelpers.WithInheritedDynamicProxy<Calculator>();
        }

        [Benchmark]
        public ICalculator WithLightInject()
        {
            return LightInjectProxyHelpers.WithLightInject();
        }
    }
}
