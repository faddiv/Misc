using BenchmarkDotNet.Running;
using System;


namespace ProxiesBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Decorate.InitLightInject();
            var target = new Calculator();
            var simple = Decorate.DecorateSimple(target);
            Console.WriteLine($"DecorateSimple: {simple.Add(1, 2)}");

            var realProxy = Decorate.WithRealProxy(new CalculatorMarshalled());
            Console.WriteLine($"WithRealProxy: {realProxy.Add(1, 2)}");

            var dispatchProxy = Decorate.WithDispatchProxy<ICalculator>(target);
            Console.WriteLine($"WithDispatchProxy: {dispatchProxy.Add(1, 2)}");

            var compositeDynamicProxy = Decorate.WithCompositeDynamicProxy<ICalculator>(target);
            Console.WriteLine($"WithCompositeDynamicProxy: {compositeDynamicProxy.Add(1, 2)}");

            var inheritedDynamicProxy = Decorate.WithInheritedDynamicProxy<Calculator>();
            Console.WriteLine($"WithInheritedDynamicProxy: {inheritedDynamicProxy.Add(1, 2)}");

            var lightInject = Decorate.WithLightInject();
            Console.WriteLine($"WithLightInject: {lightInject.Add(1, 2)}");



            var summaries = BenchmarkRunner.Run(typeof(Program).Assembly);
            
            /*foreach (var summary in summaries)
            {
                var processStartInfo = new ProcessStartInfo(
                "c:\\Program Files\\R\\R-3.6.2\\bin\\Rscript.exe",
                "BuildPlots.R")
                {
                    WorkingDirectory = summary.ResultsDirectoryPath
                };
                var process = Process.Start(processStartInfo);
                process.WaitForExit();
            }*/
        }
    }
}
