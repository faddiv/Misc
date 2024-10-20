// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using GreenDonutRelatedExperiments;

//await SubscriptionBenchmarks.TestAsync();
//await PublishBenchmarks.TestAsync();

BenchmarkRunner.Run(
[
    //typeof(StackAllocBenchmarks)
    //typeof(SubscriptionBenchmarks),
    //typeof(PublishBenchmarks),
    typeof(FalseSharingBenchmarks),
]);
