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
    public ManualResetEvent evt = new(false);

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
                evt.Set();
            }, null);

            _subV1 = _cacheV1.Subscribe<string>((cache, promise) =>
            {
                NotifiedV1 = true;
                evt.Set();
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
    public Entry NotifySubscribersBase()
    {
        evt.Reset();
        var promise = NotificationV1.Promise<string>.Create();
        var entry = new Entry(_key, promise);
        promise.TrySetResult("Result");
        evt.Set();
        if (!evt.WaitOne(50))
        {
            throw new InvalidOperationException($"{nameof(NotifySubscribersBase)} timed out.");
        }
        return entry;
    }

    [Benchmark]
    public Entry NotifySubscribersV1()
    {
        evt.Reset();
        var promise = NotificationV1.Promise<string>.Create();
        var entry = new Entry(_key, promise);
        promise.OnComplete(NotificationV1.PromiseCache.NotifySubscribers, new NotificationV1.CacheAndKey(_cacheV1, _key));
        promise.TrySetResult("Result");
        if (!Subscribe)
        {
            evt.Set();
        }
        if (!evt.WaitOne(50))
        {
            throw new InvalidOperationException($"{nameof(NotifySubscribersV1)} timed out.");
        }
        return entry;
    }

    [Benchmark]
    public Entry NotifySubscribersV2()
    {
        evt.Reset();
        var promise = Promise<string>.Create();
        var entry = new Entry(_key, promise);
        promise.NotifySubscribersOnComplete(_cache, _key);
        promise.TrySetResult("Result");
        if (!Subscribe)
        {
            evt.Set();
        }
        if (!evt.WaitOne(50))
        {
            throw new InvalidOperationException($"{nameof(NotifySubscribersV2)} timed out.");
        }
        return entry;
    }


    public static async Task TestAsync()
    {
        var benchmark = new SubscriptionBenchmarks();
        benchmark.Subscribe = true;
        benchmark.Setup();
        benchmark.NotifySubscribersBase();
        await TestV1Async(benchmark);
        await TestV2Async(benchmark);
        benchmark.Cleanup();
    }

    private static async Task TestV1Async(SubscriptionBenchmarks benchmark)
    {
        benchmark.NotifySubscribersV1();
        if (benchmark.NotifiedV1 == false)
        {
            throw new Exception($"{nameof(NotifySubscribersV1)} failed");
        }
        Console.WriteLine($"{nameof(NotifySubscribersV1)} test successfull");
    }

    private static async Task TestV2Async(SubscriptionBenchmarks benchmark)
    {
        benchmark.NotifySubscribersV2();
        if (benchmark.Notified == false)
        {
            throw new Exception($"{nameof(NotifySubscribersV2)} failed");
        }
        Console.WriteLine($"{nameof(NotifySubscribersV2)} test successfull");
    }
}
