using BenchmarkDotNet.Attributes;
using System;
using ProxiesBenchmark.CastleProxy;
using ProxiesBenchmark.DispatchProxyExample;
using ProxiesBenchmark.InterceptorExperiment;
using ProxiesBenchmark.LightInjectExample;
using ProxiesBenchmark.ManuallyImpelemntedProxy;

namespace ProxiesBenchmark.Benchmarks
{
    public class MethodCallBenchmarks : BenchmarksBase
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
        private ICalculator experimental;
        private Random rnd = new Random();
        private int a;
        private int b;

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
            experimental = ExperimentalHelpers.WithExperimental(target);
            a = rnd.Next(1000);
            b = rnd.Next(1000);
        }

        [Benchmark]
        public int DecorateSimple()
        {
            return simple.Add(a, b);
        }

        [Benchmark]
        public int WithDispatchProxy()
        {
            return dispatch.Add(a, b);
        }

        [Benchmark]
        public int WithCompositeDynamicProxy()
        {
            return composite.Add(a, b);
        }

        [Benchmark]
        public int WithInheritedDynamicProxy()
        {
            return inherited.Add(a, b);
        }

        [Benchmark]
        public int WithLightInject()
        {
            return lightInject.Add(a, b);
        }

        [Benchmark]
        public int WithExperimental()
        {
            return experimental.Add(a, b);
        }

#if NET48
        [Benchmark]
        public int WithRealProxy()
        {
            return real.Add(a, b);
        }
#endif
    }
}
