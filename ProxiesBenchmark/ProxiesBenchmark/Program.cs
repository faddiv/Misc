using BenchmarkDotNet.Running;
using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using ProxiesBenchmark.Benchmarks;
using ProxiesBenchmark.CastleProxy;
using ProxiesBenchmark.DispatchProxyExample;
using ProxiesBenchmark.InterceptorExperiment;
using ProxiesBenchmark.LightInjectExample;
using ProxiesBenchmark.ManuallyImpelemntedProxy;


namespace ProxiesBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            LightInjectProxyHelpers.InitLightInject();
            var target = new Calculator();
            var simple = ManuallyImplementedProxyHelpers.DecorateSimple(target);
            Console.WriteLine($"DecorateSimple: {simple.Add(1, 2)}");

#if NET48
            var realProxy = SystemRuntimeRemotingProxies.RealProxyHelpers.WithRealProxy(new CalculatorMarshalled());
            Console.WriteLine($"WithRealProxy: {realProxy.Add(1, 2)}");
#endif

            var dispatchProxy = DispatchProxyHelpers.WithDispatchProxy<ICalculator>(target);
            Console.WriteLine($"WithDispatchProxy: {dispatchProxy.Add(1, 2)}");

            var compositeDynamicProxy = CastleDynamicProxyHelpers.WithCompositeDynamicProxy<ICalculator>(target);
            Console.WriteLine($"WithCompositeDynamicProxy: {compositeDynamicProxy.Add(1, 2)}");

            var inheritedDynamicProxy = CastleDynamicProxyHelpers.WithInheritedDynamicProxy<Calculator>();
            Console.WriteLine($"WithInheritedDynamicProxy: {inheritedDynamicProxy.Add(1, 2)}");

            var lightInject = LightInjectProxyHelpers.WithLightInject();
            Console.WriteLine($"WithLightInject: {lightInject.Add(1, 2)}");

            var experimental = ExperimentalHelpers.WithExperimental(target);
            Console.WriteLine($"WithExperimental: {experimental.Add(1, 2)}");

            BenchmarkRunner.Run<MethodCallBenchmarks>(
                DefaultConfig.Instance
                    .AddJob(Job.Default.WithRuntime(CoreRuntime.Core80))
                    //.AddJob(Job.Default.WithRuntime(ClrRuntime.Net48))
                    .AddDiagnoser(MemoryDiagnoser.Default)
                    .WithArtifactsPath(@"..\..\..\..\..\Results"));
        }
    }
}
