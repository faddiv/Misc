using BenchmarkDotNet.Attributes;
using GreenDonutRelatedExperiments.NotificationBenchParts;
using GreenDonutRelatedExperiments.NotificationCommon;

namespace GreenDonutRelatedExperiments;

[MemoryDiagnoser]
public class SubscriptionBenchmarks
{
    private readonly NotificationV1.PromiseCache _cacheV1 = new NotificationV1.PromiseCache(10);
    private readonly PromiseCache _cache = new PromiseCache(10);
    private PromiseCacheKey _key = new PromiseCacheKey("Type", "Key");
    private IDisposable? _sub = null;
    private IDisposable? _subV1 = null;

    public bool Notified { get; set; } = false;
    public bool NotifiedV1 { get; set; } = false;

    [Params(false, true)]
    public bool Subscribe { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        if (Subscribe)
        {
            _sub = _cache.Subscribe<string>((cache, promise) =>
            {
                Notified = true;
            }, null);

            _subV1 = _cacheV1.Subscribe<string>((cache, promise) =>
            {
                NotifiedV1 = true;
            }, null);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _sub?.Dispose();
        _subV1?.Dispose();
    }

    [Benchmark]
    public async Task<Entry> NotifySubscribersBase()
    {
        var promise = NotificationV1.Promise<string>.Create();
        var entry = new Entry(_key, promise);
        promise.TrySetResult("Result");
        await Task.Yield();
        return entry;
    }

    [Benchmark]
    public async Task<Entry> NotifySubscribersV1()
    {
        var promise = NotificationV1.Promise<string>.Create();
        var entry = new Entry(_key, promise);
        promise.OnComplete(NotificationV1.PromiseCache.NotifySubscribers, new NotificationV1.CacheAndKey(_cacheV1, _key));
        promise.TrySetResult("Result");
        await Task.Yield();
        return entry;
    }

    [Benchmark]
    public async Task<Entry> NotifySubscribersV2()
    {
        var promise = Promise<string>.Create();
        var entry = new Entry(_key, promise);
        promise.NotifySubscribersOnComplete(_cache, _key);
        promise.TrySetResult("Result");
        await Task.Yield();
        return entry;
    }


    public static async Task Test()
    {
        var benchmark = new SubscriptionBenchmarks();
        benchmark.Subscribe = true;
        benchmark.Setup();
        await TestV1(benchmark);
        await TestV2(benchmark);
        benchmark.Cleanup();
    }

    private static async Task TestV1(SubscriptionBenchmarks benchmark)
    {
        await benchmark.NotifySubscribersV1();
        if (benchmark.NotifiedV1 == false)
        {
            throw new Exception($"{nameof(NotifySubscribersV1)} failed");
        }
        Console.WriteLine($"{nameof(NotifySubscribersV1)} test successfull");
    }

    private static async Task TestV2(SubscriptionBenchmarks benchmark)
    {
        await benchmark.NotifySubscribersV2();
        if (benchmark.Notified == false)
        {
            throw new Exception($"{nameof(NotifySubscribersV2)} failed");
        }
        Console.WriteLine($"{nameof(NotifySubscribersV2)} test successfull");
    }
}
