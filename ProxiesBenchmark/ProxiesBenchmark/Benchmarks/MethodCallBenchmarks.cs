using BenchmarkDotNet.Attributes;
using Foxy.Testing.EntityFrameworkCore.BenchmarkDotNet;
using System;

namespace ProxiesBenchmark.Benchmarks
{
    [ArtifactsPath(".\\Benchmarks")]
    public class MethodCallBenchmarks : BenchmarksBase
    {
        private Calculator target;
        private CalculatorMarshalled target2;
        private ICalculator simple;
        private ICalculator real;
        private ICalculator dispatch;
        private ICalculator composite;
        private ICalculator inherited;
        private ICalculator lightInject;
        private Random rnd = new Random();
        private int a;
        private int b;
        [GlobalSetup]
        public void Setup()
        {
            Decorate.InitLightInject();
            target = new Calculator();
            target2 = new CalculatorMarshalled();
            simple = Decorate.DecorateSimple(target);
            real = Decorate.WithRealProxy(target2);
            dispatch = Decorate.WithDispatchProxy<ICalculator>(target);
            composite = Decorate.WithCompositeDynamicProxy<ICalculator>(target);
            inherited = Decorate.WithInheritedDynamicProxy<Calculator>();
            lightInject = Decorate.WithLightInject();
            a = rnd.Next(1000);
            b = rnd.Next(1000);
        }

        [Benchmark]
        public int DecorateSimple()
        {
            return simple.Add(a, b);
        }

        [Benchmark]
        public int WithRealProxy()
        {
            return real.Add(a, b);
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
    }
}
