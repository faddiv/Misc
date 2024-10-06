// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using GreenDonutRelatedExperiments;

await SubscriptionBenchmarks.TestAsync();
await PublishBenchmarks.TestAsync();
BenchmarkRunner.Run(
[
    typeof(SubscriptionBenchmarks),
    typeof(PublishBenchmarks),
]);
