// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using GreenDonutRelatedExperiments;

await SubscriptionBenchmarks.Test();
BenchmarkRunner.Run<SubscriptionBenchmarks>();
