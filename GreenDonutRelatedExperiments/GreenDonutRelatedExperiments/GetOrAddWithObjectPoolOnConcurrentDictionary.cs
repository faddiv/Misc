using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.ObjectPool;

namespace GreenDonutRelatedExperiments;

[MemoryDiagnoser]
public class GetOrAddWithObjectPoolOnConcurrentDictionary
{
    private ConcurrentDictionary<string, Values> _dictionary = null!;
    private static readonly object _obj = new();
    private readonly ObjectPool<HelperContainer> _pool = new DefaultObjectPool<HelperContainer>(new Policy());
    private HelperContainer _container = null!;

    [GlobalSetup]
    public void Setup()
    {
        _container = _pool.Get();
        _dictionary = new ConcurrentDictionary<string, Values>();
        for (var i = 0; i < 100; i++)
        {
            var key = $"key{i}";
            _dictionary.TryAdd(key, new Values(key, i));
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _pool.Return(_container);
    }

    [Benchmark]
    public Values GetOrAddState()
    {
        try
        {
            var holder = _pool.Get();
            holder.Payload = _obj;
            Values? v;
            try
            {
                v = _dictionary.GetOrAdd("test", static (k, h) =>
                {
                    h.Values = new Values(k, h.Payload!);
                    return h.Values;
                }, holder);
            }
            finally
            {
                _pool.Return(holder);
            }

            return v;
        }
        finally
        {
            _dictionary.TryRemove("test", out _);
        }
    }

    [Benchmark]
    public Values GetOrAddCachedState()
    {
        try
        {
            Values? v;
            var holder = _pool.Get();
            holder.Payload = _obj;
            try
            {
                v = _dictionary.GetOrAdd("key55", static (k, h) =>
                {
                    h.Values = new Values(k, h.Payload!);
                    return h.Values;
                }, holder);
            }
            finally
            {
                _pool.Return(holder);
            }

            return v;
        }
        finally
        {
            _dictionary.TryRemove("test", out _);
        }
    }

    [Benchmark]
    public Values TryGetOrAddState()
    {
        try
        {
            if (_dictionary.TryGetValue("test", out var result))
            {
                return result;
            }

            var value = new Values("test", _obj);
            result = _dictionary.GetOrAdd("test", value);
            return result;
        }
        finally
        {
            _dictionary.TryRemove("test", out _);
        }
    }

    [Benchmark]
    public Values TryGetOrAddCachedState()
    {
        try
        {
            if (_dictionary.TryGetValue("key55", out var result))
            {
                return result;
            }

            var value = new Values("test", _obj);
            result = _dictionary.GetOrAdd("test", value);
            return result;
        }
        finally
        {
            _dictionary.TryRemove("test", out _);
        }
    }
}
