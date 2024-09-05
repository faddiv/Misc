// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using PartialStringComparer.Benchmarks;

Console.WriteLine("Hello, World!");

BenchmarkRunner.Run<InterpolatedComparerBenchmark>();
