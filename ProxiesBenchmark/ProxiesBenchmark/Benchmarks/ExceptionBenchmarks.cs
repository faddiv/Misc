using BenchmarkDotNet.Attributes;
using System;
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
    public class ExceptionBenchmarks : BenchmarksBase
    {
        private Calculator target;
#if NET48
        private CalculatorMarshalled target2;
        private ICalculator real;
#endif
        private ICalculator simple;
        private ICalculator dispatch;
        private ICalculator composite;
        private ICalculator inherited;
        private ICalculator lightInject;
        private Random rnd = new Random();
        private string a;

        [GlobalSetup]
        public void Setup()
        {
            LightInjectProxyHelpers.InitLightInject();
            target = new Calculator();
            simple = ManuallyImplementedProxyHelpers.DecorateSimple(target);
#if NET48
            target2 = new CalculatorMarshalled();
            real = SystemRuntimeRemotingProxies.RealProxyHelpers.WithRealProxy(target2);
#endif
            dispatch = DispatchProxyHelpers.WithDispatchProxy<ICalculator>(target);
            composite = CastleDynamicProxyHelpers.WithCompositeDynamicProxy<ICalculator>(target);
            inherited = CastleDynamicProxyHelpers.WithInheritedDynamicProxy<Calculator>();
            lightInject = LightInjectProxyHelpers.WithLightInject();
            a = rnd.Next(1000).ToString();
        }

        [Benchmark]
        public void DecorateSimple()
        {
            try
            {
                simple.Throw(a);
            }
            catch (Exception)
            {
            }
        }
#if NET48
        [Benchmark]
        public void WithRealProxy()
        {
            try
            {
                real.Throw(a);
            }
            catch (Exception)
            {
            }
        }
#endif
        [Benchmark]
        public void WithDispatchProxy()
        {
            try
            {
                dispatch.Throw(a);
            }
            catch (Exception)
            {
            }
        }

        [Benchmark]
        public void WithCompositeDynamicProxy()
        {
            try
            {
                composite.Throw(a);
            }
            catch (Exception)
            {
            }
        }

        [Benchmark]
        public void WithInheritedDynamicProxy()
        {
            try
            {
                inherited.Throw(a);
            }
            catch (Exception)
            {
            }
        }

        [Benchmark]
        public void WithLightInject()
        {
            try
            {
                lightInject.Throw(a);
            }
            catch (Exception)
            {
            }
        }
    }
}
