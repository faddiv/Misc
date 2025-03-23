using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace GreenDonutRelatedExperiments;

[MemoryDiagnoser]
public class ConcurrentParallelAddToDictionary
{
    private const int Elements = 500;
    private readonly Task[] _tasks = new Task[Elements];
    private readonly object _lock = new();
    private readonly string[] _results = new string[Elements];
    private ConcurrentDictionary<int, string> _concurrentDictionary = new();

    // ReSharper disable once CollectionNeverQueried.Local
    private Dictionary<int, string> _dictionary = new();

    [Benchmark]
    public async Task<string[]> AddToConcurrentDictionary()
    {
        _concurrentDictionary = new ConcurrentDictionary<int, string>();
        for (int i = 0; i < Elements; i++)
        {
            _tasks[i] = Task.Factory.StartNew(Implementation, i);
        }

        await Task.WhenAll(_tasks);
        return _results;

        void Implementation(object? state)
        {
            var key = (int)(state ?? throw new ArgumentNullException(nameof(state)));
            _results[key] = _concurrentDictionary.GetOrAdd(key, k => k.ToString());
        }
    }

    [Benchmark]
    public async Task<string[]> AddToDictionaryWithLock()
    {
        lock (_lock)
        {
            _dictionary = new Dictionary<int, string>();
        }

        for (int i = 0; i < Elements; i++)
        {
            _tasks[i] = Task.Factory.StartNew(Implementation, i);
        }

        await Task.WhenAll(_tasks);
        return _results;

        void Implementation(object? state)
        {
            var key = (int)(state ?? throw new ArgumentNullException(nameof(state)));
            var value = key.ToString();
            lock (_lock)
            {
                _dictionary.Add(key, value);
            }

            _results[key] = value;
        }
    }

    [Benchmark]
    public async Task<string[]> AddToDictionaryWithLockAndParallel()
    {
        lock (_lock)
        {
            _dictionary = new Dictionary<int, string>();
        }

        await Parallel.ForAsync(0, Elements, Implementation);
        return _results;

        ValueTask Implementation(int j, CancellationToken token)
        {
            var value = j.ToString();
            lock (_lock)
            {
                _dictionary.Add(j, value);
            }

            _results[j] = value;
            return ValueTask.CompletedTask;
        }
    }

    [Benchmark]
    public async Task<string[]> AddToConcurrentDictionaryWithParallel()
    {
        _concurrentDictionary = new ConcurrentDictionary<int, string>();
        await Parallel.ForAsync(0, Elements, Implementation);
        return _results;

        ValueTask Implementation(int j, CancellationToken token)
        {
            var value = j.ToString();
            _results[j] = _concurrentDictionary.GetOrAdd(j, value);

            return ValueTask.CompletedTask;
        }
    }

    [Benchmark]
    public async Task<string[]> AddToDictionarySerial()
    {
        lock (_lock)
        {
            _dictionary = new Dictionary<int, string>();
        }

        for (int i = 0; i < Elements; i++)
        {
            Implementation(i);
        }

        return _results;

        void Implementation(int j)
        {
            var value = j.ToString();
            lock (_lock)
            {
                _dictionary.Add(j, value);
            }

            _results[j] = value;
        }
    }

    [Benchmark]
    public async Task<string[]> AddToConcurrentDictionarySerial()
    {
        _concurrentDictionary = new ConcurrentDictionary<int, string>();
        for (int i = 0; i < Elements; i++)
        {
            Implementation(i);
        }

        return _results;

        void Implementation(int j)
        {
            var value = j.ToString();
            _results[j] = _concurrentDictionary.GetOrAdd(j, value);
        }
    }
}
/*
Elements: 50
    | Method                                | Mean     | Error    | StdDev   | Median   | Gen0   | Allocated |
    |-------------------------------------- |---------:|---------:|---------:|---------:|-------:|----------:|
    | AddToConcurrentDictionary             | 28.35 us | 0.219 us | 0.183 us | 28.40 us | 3.2959 |  13.31 KB |
    | AddToDictionaryWithLock               | 54.30 us | 2.494 us | 7.354 us | 57.63 us | 3.0518 |  12.13 KB |
    | AddToDictionaryWithParallel           | 76.86 us | 1.532 us | 4.445 us | 78.20 us | 1.2207 |   4.94 KB |
    | AddToConcurrentDictionaryWithParallel | 23.12 us | 0.460 us | 0.852 us | 22.81 us | 1.5564 |   6.23 KB |
*/

/*
Elements: 500
    | Method                                | Mean      | Error    | StdDev    | Gen0    | Gen1   | Allocated |
    |-------------------------------------- |----------:|---------:|----------:|--------:|-------:|----------:|
    | AddToConcurrentDictionary             | 216.3 us  | 4.32 us  | 7.33 us   | 37.5977 | 0.4883 | 150.69 KB |
    | AddToDictionaryWithLock               | 352.7 us  | 6.94 us  | 7.12 us   | 33.2031 | 0.4883 | 127.68 KB |
    | AddToDictionaryWithParallel           | 272.3 us  | 1.03 us  | 0.96 us   | 15.1367 | 0.4883 |  53.67 KB |
    | AddToConcurrentDictionaryWithParallel | 150.7 us  | 1.17 us  | 1.10 us   | 19.5313 | 0.4883 |  76.89 KB |
    | AddToDictionarySerial                 |  15.89 us | 0.102 us |  0.091 us | 13.0310 | 0.0305 |  53.29 KB |
    | AddToConcurrentDictionarySerial       |  38.10 us | 0.320 us |  0.299 us | 18.4326 |      - |  75.29 KB |
*/
