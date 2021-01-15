using BenchmarkDotNet.Attributes;
using Foxy.Testing.EntityFrameworkCore.BenchmarkDotNet;
using System;

namespace ProxiesBenchmark.Benchmarks
{
    [ArtifactsPath(".\\Benchmarks")]
    public class ExceptionBenchmarks : BenchmarksBase
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
        private string a;
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
