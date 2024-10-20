using BenchmarkDotNet.Attributes;
using GreenDonutRelatedExperiments.NotificationBenchParts;
using GreenDonutRelatedExperiments.NotificationCommon;

namespace GreenDonutRelatedExperiments;

[MemoryDiagnoser]
public class PublishBenchmarks
{
    private const int _subscribtionCount = 16;
    private readonly NotificationV1.PromiseCache _cacheV1 = new NotificationV1.PromiseCache(10);
    private readonly PromiseCache _cache = new PromiseCache(10);
    private PromiseCacheKey _key = new PromiseCacheKey("Type", "Key");
    private readonly List<IDisposable> _subs = new List<IDisposable>();

    public bool[] Notifieds { get; } = new bool[_subscribtionCount];
    public ManualResetEvent evt = new(false);

    [Params(false, true)]
    public bool Subscribe { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        if (Subscribe)
        {
            for (int i = 0; i < _subscribtionCount - 1; i++)
            {
                var index = i;
                var sub = _cache.Subscribe<string>((cache, promise) =>
                {
                    Notifieds[index] = true;
                }, null);
                _subs.Add(sub);

                sub = _cacheV1.Subscribe<string>((cache, promise) =>
                {
                    Notifieds[index] = true;
                }, null);
                _subs.Add(sub);
            }
            {
                var sub = _cache.Subscribe<string>((cache, promise) =>
                {
                    Notifieds[_subscribtionCount - 1] = true;
                    evt.Set();
                }, null);
                sub = _cacheV1.Subscribe<string>((cache, promise) =>
                {
                    Notifieds[_subscribtionCount - 1] = true;
                    evt.Set();
                }, null);
                _subs.Add(sub);
            }
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _subs.ForEach(d => d.Dispose());
    }

    [Benchmark]
    public Entry ExecuteNotifycationV1()
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
            throw new InvalidOperationException($"{nameof(ExecuteNotifycationV1)} timed out.");
        }
        return entry;
    }

    [Benchmark]
    public Entry ExecuteNotifycationV2()
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
            throw new InvalidOperationException($"{nameof(ExecuteNotifycationV2)} timed out.");
        }
        return entry;
    }


    public static async Task TestAsync()
    {
        var benchmark = new PublishBenchmarks();
        benchmark.Subscribe = true;
        benchmark.Setup();
        await TestV1Async(benchmark);
        for (int i = 0; i < benchmark.Notifieds.Length; i++)
        {
            benchmark.Notifieds[i] = false;
        };
        await TestV2Async(benchmark);
        benchmark.Cleanup();
    }

    private static async Task TestV1Async(PublishBenchmarks benchmark)
    {
        benchmark.ExecuteNotifycationV1();
        if (benchmark.Notifieds.All(e => e == true) == false)
        {
            throw new Exception($"{nameof(ExecuteNotifycationV1)} failed");
        }
        Console.WriteLine($"{nameof(ExecuteNotifycationV1)} test successfull");
    }

    private static async Task TestV2Async(PublishBenchmarks benchmark)
    {
        benchmark.ExecuteNotifycationV2();
        if (benchmark.Notifieds.All(e => e == true) == false)
        {
            throw new Exception($"{nameof(ExecuteNotifycationV2)} failed");
        }
        Console.WriteLine($"{nameof(ExecuteNotifycationV2)} test successfull");
    }
}
