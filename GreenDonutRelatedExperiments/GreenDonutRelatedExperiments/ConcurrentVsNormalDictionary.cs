using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace GreenDonutRelatedExperiments;

[MemoryDiagnoser]
public class ConcurrentVsNormalDictionary
{
    private readonly ConcurrentDictionary<string, Values> _concurrentDictionary = new();
    private readonly ConcurrentDictionary<string, Values> _concurrentDictionary2 = new();
    private readonly Dictionary<string, Values> _dictionary = new();
    private readonly Dictionary<string, Values> _dictionary2 = new();
    private readonly ReaderWriterLockSlim _locker = new();
    private string[] _keys = null!;
    private Values[] _values = null!;

    [Params(100, 1000, 10000, 100000)]
    public static int CountItem { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _keys = new string[CountItem];
        _values = new Values[CountItem];
        _concurrentDictionary.Clear();
        _concurrentDictionary2.Clear();
        _dictionary.Clear();
        _dictionary2.Clear();
        for (int i = 0; i < CountItem; i++)
        {
            _keys[i] = $"key{i}";
            _values[i] = new Values(_keys[i], i);
            _concurrentDictionary.TryAdd(_keys[i], _values[i]);
            _dictionary.TryAdd(_keys[i], _values[i]);
        }
    }

    [Benchmark]
    public Values GetFromDictionary()
    {
        _locker.EnterReadLock();
        try
        {
            var rnd = Random.Shared.Next(CountItem);
            return _dictionary[_keys[rnd]];
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    [Benchmark]
    public Values GetFromConcurrentDictionary()
    {
        var rnd = Random.Shared.Next(CountItem);
        return _concurrentDictionary[_keys[rnd]];
    }

    [Benchmark]
    public Values WriteToDictionary()
    {
        _locker.EnterWriteLock();
        try
        {
            for (int i = 0; i < CountItem; i++)
            {
                _dictionary2.TryAdd(_keys[i], new Values(_keys[i], _values[i]));
            }
        }
        finally
        {
            _locker.ExitWriteLock();
        }

        _locker.EnterReadLock();
        try
        {
            var rnd = Random.Shared.Next(CountItem);
            return _dictionary2[_keys[rnd]];
        }
        finally
        {
            _locker.ExitReadLock();
            _dictionary2.Clear();
        }
    }

    [Benchmark]
    public Values WriteToConcurrentDictionary()
    {
        for (int i = 0; i < CountItem; i++)
        {
            _concurrentDictionary2.TryAdd(_keys[i], _values[i]);
        }

        try
        {
            var rnd = Random.Shared.Next(CountItem);
            return _concurrentDictionary2[_keys[rnd]];
        }
        finally
        {
            _concurrentDictionary2.Clear();
        }
    }
}

/*
| Method                      | CountItem | Mean             | Error          | StdDev         | Gen0      | Gen1      | Gen2     | Allocated  |
|---------------------------- |---------- |-----------------:|---------------:|---------------:|----------:|----------:|---------:|-----------:|
| GetFromDictionary           | 100       |         37.58 ns |       0.188 ns |       0.167 ns |         - |         - |        - |          - |
| GetFromConcurrentDictionary | 100       |         25.06 ns |       0.231 ns |       0.193 ns |         - |         - |        - |          - |
| WriteToDictionary           | 100       |      2,479.51 ns |      15.346 ns |      13.604 ns |    0.7629 |         - |        - |     3200 B |
| WriteToConcurrentDictionary | 100       |     75,894.63 ns |     349.800 ns |     292.099 ns |    6.8359 |         - |        - |    28768 B |
| GetFromDictionary           | 1000      |         43.73 ns |       0.289 ns |       0.270 ns |         - |         - |        - |          - |
| GetFromConcurrentDictionary | 1000      |         24.92 ns |       0.245 ns |       0.205 ns |         - |         - |        - |          - |
| WriteToDictionary           | 1000      |     32,311.05 ns |     508.455 ns |     475.609 ns |    7.6294 |         - |        - |    32000 B |
| WriteToConcurrentDictionary | 1000      |    254,966.94 ns |   1,741.735 ns |   1,629.220 ns |   51.2695 |         - |        - |   215504 B |
| GetFromDictionary           | 10000     |         61.50 ns |       1.021 ns |       0.955 ns |         - |         - |        - |          - |
| GetFromConcurrentDictionary | 10000     |         39.08 ns |       0.131 ns |       0.117 ns |         - |         - |        - |          - |
| WriteToDictionary           | 10000     |    300,465.69 ns |   1,225.645 ns |   1,146.469 ns |   74.7070 |   18.5547 |        - |   320000 B |
| WriteToConcurrentDictionary | 10000     |  2,597,175.08 ns |  22,681.609 ns |  20,106.656 ns |  265.6250 |  199.2188 | 132.8125 |  1707021 B |
| GetFromDictionary           | 100000    |        181.67 ns |       3.474 ns |       3.412 ns |         - |         - |        - |          - |
| GetFromConcurrentDictionary | 100000    |        171.15 ns |       2.442 ns |       1.906 ns |         - |         - |        - |          - |
| WriteToDictionary           | 100000    |  4,909,471.75 ns |  83,443.081 ns |  69,678.752 ns |  515.6250 |  507.8125 |        - |  3200000 B |
| WriteToConcurrentDictionary | 100000    | 36,099,115.31 ns | 608,293.745 ns | 539,236.561 ns | 1642.8571 | 1000.0000 | 285.7143 | 11086682 B |

*/
