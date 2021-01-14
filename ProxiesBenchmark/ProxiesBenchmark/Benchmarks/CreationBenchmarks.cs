using BenchmarkDotNet.Attributes;
using Foxy.Testing.EntityFrameworkCore.BenchmarkDotNet;

namespace ProxiesBenchmark.Benchmarks
{
    [ArtifactsPath(".\\Benchmarks")]
    public class CreationBenchmarks : BenchmarksBase
    {
        private Calculator target;
        private CalculatorMarshalled target2;
        [GlobalSetup]
        public void Setup()
        {
            target = new Calculator();
            target2 = new CalculatorMarshalled();
        }

        [Benchmark]
        public ICalculator DecorateSimple()
        {
            return Decorate.DecorateSimple(target);
        }

        [Benchmark]
        public ICalculator WithRealProxy()
        {
            return Decorate.WithRealProxy(target2);
        }

        [Benchmark]
        public ICalculator WithDispatchProxy()
        {
            return Decorate.WithDispatchProxy<ICalculator>(target);
        }

        [Benchmark]
        public ICalculator WithCompositeDynamicProxy()
        {
            return Decorate.WithCompositeDynamicProxy<ICalculator>(target);
        }

        [Benchmark]
        public ICalculator WithInheritedDynamicProxy()
        {
            return Decorate.WithInheritedDynamicProxy<Calculator>();
        }
    }
}
