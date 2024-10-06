using BenchmarkDotNet.Attributes;
using System.Collections.Concurrent;

namespace GreenDonutRelatedExperiments;

[MemoryDiagnoser]
public class GetOrAddCasesOnConcurrentDictionary
{
    private ConcurrentDictionary<string, Values> _dictionary = null!;
    private static object obj = new object();
    [GlobalSetup]
    public void Setup()
    {
        _dictionary = new ConcurrentDictionary<string, Values>();
        for (int i = 0; i < 100; i++)
        {
            string key = $"key{i}";
            _dictionary.TryAdd(key, new Values(key, i));
        }
    }

    [Benchmark]
    public Values GetOrAddState()
    {
        Values v = _dictionary.GetOrAdd("test", static (k, s) => new Values(k, s), obj);
        _dictionary.TryRemove("test", out _);
        return v;
    }

    [Benchmark]
    public object GetOrAdd()
    {
        object v = _dictionary.GetOrAdd("test", static (key) => new Values(key, obj));
        _dictionary.TryRemove("test", out _);
        return v;
    }

    [Benchmark]
    public Values GetThenTryAdd()
    {
        if(_dictionary.TryGetValue("test", out Values? result))
        {
            return result;
        }

        var value = new Values("test", obj);
        result = _dictionary.GetOrAdd("test", value);
        _dictionary.TryRemove("test", out _);
        return result;
    }

    [Benchmark]
    public Values GetOrAddIsNew()
    {
        Values? created = null;
        Values v = _dictionary.GetOrAdd("test", (k) => {
            return created = new Values(k, obj);
        });
        _dictionary.TryRemove("test", out _);
        if (ReferenceEquals(created, v))
        {
            return v;
        }
        return created ?? new Values("test", obj);
    }

    [Benchmark]
    public Values GetOrAddAlwaysCreate()
    {
        var created = new Values("test", obj);
        Values v = _dictionary.GetOrAdd("test", created);
        _dictionary.TryRemove("test", out _);
        if (ReferenceEquals(created, v))
        {
            return v;
        }
        return created ?? new Values("test", obj);
    }

    [Benchmark]
    public Values GetOrAddCachedState()
    {
        Values v = _dictionary.GetOrAdd("key55", static (k, s) => new Values(k, s), obj);
        _dictionary.TryRemove("test", out _);
        return v;
    }
}

public record class Values(string Key, object Value);
