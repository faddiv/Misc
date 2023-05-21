// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using ViteCommerce.Benchmarks;

Console.WriteLine("Hello, World!");
//BenchmarkRunner.Run<BenchmarkForDomainResponses>();
//BenchmarkRunner.Run<BenchmarkForMediators>();
//BenchmarkRunner.Run<AsyncOperationBenchmark>();
BenchmarkRunner.Run<BenchmarkForPipelines>();
