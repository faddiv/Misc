// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using GreenDonutRelatedExperiments;
using System.Collections.Concurrent;

//await SubscriptionBenchmarks.TestAsync();
//await PublishBenchmarks.TestAsync();


BenchmarkRunner.Run(
[
    //typeof(StackAllocBenchmarks)
    //typeof(SubscriptionBenchmarks),
    //typeof(PublishBenchmarks),
    //typeof(FalseSharingBenchmarks),
    //typeof(GetOrAddCasesOnConcurrentDictionary),
    //typeof(GetOrAddWithObjectPoolOnConcurrentDictionary)
    //typeof(ConcurrentVsNormalDictionary)
    typeof(ConcurrentParallelAddToDictionary)
]);
/* Getting the Processor id on which the Thread runs. Maybe this is good for true thread sharing
ConcurrentDictionary<int, int> processors = new();
var c = 10000;
var tasks = new Task[c];
for (int i = 0; i < c; i++)
{
    tasks[i] = Task.Run(WriteMyProcessor);
}
Task.WaitAll(tasks);

Console.WriteLine($"Used processors: {string.Join(',', processors.ToArray())}");

void WriteMyProcessor()
{
    var id = Thread.GetCurrentProcessorId();
    var th = Task.CurrentId;
    processors.AddOrUpdate(id, 1, (k, v) => v+1);
    Console.WriteLine($"Task{th} on processor: {id}");
}
*/
