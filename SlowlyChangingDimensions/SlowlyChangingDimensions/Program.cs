// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using SlowlyChangingDimensions;

Console.WriteLine("Hello, World!");
//Scd1Seed.Seed();
//Scd2Seed.Seed();
var benchmark = typeof(Benchy);
BenchmarkRunner.Run(benchmark);
