using BenchmarkDotNet.Attributes;
using System.Collections.Concurrent;

namespace GreenDonutRelatedExperiments;

[MemoryDiagnoser]
public class GetOrAddCasesOnConcurrentDictionary
{
    private ConcurrentDictionary<string, Values> _dictionary = null!;
    private static readonly object _obj = new();
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
        var v = _dictionary.GetOrAdd("test", static (k, s) => new Values(k, s), _obj);
        _dictionary.TryRemove("test", out _);
        return v;
    }

    [Benchmark]
    public object GetOrAdd()
    {
        object v = _dictionary.GetOrAdd("test", static (key) => new Values(key, _obj));
        _dictionary.TryRemove("test", out _);
        return v;
    }

    [Benchmark]
    public Values GetThenTryAdd()
    {
        if(_dictionary.TryGetValue("test", out var result))
        {
            return result;
        }

        var value = new Values("test", _obj);
        result = _dictionary.GetOrAdd("test", value);
        _dictionary.TryRemove("test", out _);
        return result;
    }

    [Benchmark]
    public Values GetOrAddIsNew()
    {
        Values created = default;
        var v = _dictionary.GetOrAdd("test", (k) => {
            return created = new Values(k, _obj);
        });
        _dictionary.TryRemove("test", out _);
        if (created == v)
        {
            return v;
        }
        return created;
    }

    [Benchmark]
    public Values GetOrAddAlwaysCreate()
    {
        var created = new Values("test", _obj);
        var v = _dictionary.GetOrAdd("test", created);
        _dictionary.TryRemove("test", out _);
        if (created == v)
        {
            return v;
        }
        return created;
    }

    [Benchmark]
    public Values GetOrAddCachedState()
    {
        var v = _dictionary.GetOrAdd("key55", static (k, s) => new Values(k, s), _obj);
        _dictionary.TryRemove("test", out _);
        return v;
    }
}
