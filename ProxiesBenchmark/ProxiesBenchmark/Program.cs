using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ProxiesBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var target = new Calculator();
            var simple = Decorate.DecorateSimple(target);
            Console.WriteLine($"DecorateSimple: {simple.Add(1, 2)}");

            var realProxy = Decorate.WithRealProxy(target);
            Console.WriteLine($"WithRealProxy: {realProxy.Add(1, 2)}");

            var dispatchProxy = Decorate.WithDispatchProxy<ICalculator>(target);
            Console.WriteLine($"WithDispatchProxy: {dispatchProxy.Add(1, 2)}");

            var compositeDynamicProxy = Decorate.WithCompositeDynamicProxy<ICalculator>(target);
            Console.WriteLine($"WithCompositeDynamicProxy: {compositeDynamicProxy.Add(1, 2)}");

            var inheritedDynamicProxy = Decorate.WithInheritedDynamicProxy<Calculator>();
            Console.WriteLine($"WithInheritedDynamicProxy: {inheritedDynamicProxy.Add(1, 2)}");

            var summaries = BenchmarkRunner.Run(typeof(Program).Assembly);
            foreach (var summary in summaries)
            {
                var processStartInfo = new ProcessStartInfo(
                "c:\\Program Files\\R\\R-3.6.2\\bin\\Rscript.exe",
                "BuildPlots.R")
                {
                    WorkingDirectory = summary.ResultsDirectoryPath
                };
                var process = Process.Start(processStartInfo);
                process.WaitForExit();
            }
        }
    }
}
