// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using Benchmarks.ResultShape;

Console.WriteLine("Hello, World!");
BenchmarkRunner.Run<ResultShapeBenchmark>();
