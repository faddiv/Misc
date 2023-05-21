using BenchmarkDotNet.Attributes;

namespace ViteCommerce.Benchmarks;

[MemoryDiagnoser]
public class AsyncOperationBenchmark
{
    [Benchmark]
    public async Task<int> WithAwait_Real()
    {
        var i = await RealAsyncTask(0).ConfigureAwait(false);
        return await RealAsyncTask(i).ConfigureAwait(false);
    }

    [Benchmark]
    public Task<int> WithChaining_Real()
    {
        return RealAsyncTask(0).ContinueWith(i => RealAsyncTask(i.Result)).Unwrap();
    }

    [Benchmark]
    public async Task<int> WithAwait_Completed()
    {
        var i = await FakeAsyncTask(0).ConfigureAwait(false);
        return await FakeAsyncTask(i).ConfigureAwait(false);
    }

    [Benchmark]
    public Task<int> WithChaining_Completed()
    {
        return FakeAsyncTask(0).ContinueWith(i => FakeAsyncTask(i.Result)).Unwrap();
    }

    public static async Task<int> RealAsyncTask(int i)
    {
        await Task.Delay(10);
        return i + 1;
    }

    public static Task<int> FakeAsyncTask(int i)
    {
        return Task.FromResult(i + 1);
    }
}
