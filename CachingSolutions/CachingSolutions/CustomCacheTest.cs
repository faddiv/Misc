// See https://aka.ms/new-console-template for more information

using FluentAssertions;
using Xunit;

namespace CachingSolutions;

public class CustomCacheTest
{
    [Fact]
    public async Task CreatesCacheItem()
    {
        ICustomCache cache = new CustomCache();
        var result = await cache.GetOrCreate("a", "a", (entry, args) => Task.FromResult("x" + args));

        result.Should().Be("xa");
    }
    [Fact]
    public async Task CreatesCacheItemAsync()
    {
        ICustomCache cache = new CustomCache();
        var result = await cache.GetOrCreate("a", "a", async (entry, args) =>
        {
            await Task.Delay(10);
            return "x" + args;
        });

        result.Should().Be("xa");
    }

    [Fact]
    public async Task CreateRunsOnlyOnce_SimulatingLongAwait()
    {
        ICustomCache cache = new CustomCache();
        int i = 0;
        int j = 0;
        var results = await Task.WhenAll(
            cache.GetOrCreate("a", "a", async (entry, args) =>
            {
                await Task.Delay(100);
                Interlocked.Increment(ref i);
                return "x" + args;
            }),
            cache.GetOrCreate("a", "b", async (entry, args) =>
            {
                await Task.Delay(100);
                Interlocked.Increment(ref j);
                return "x" + args;
            }),
            cache.GetOrCreate("a", "b", async (entry, args) =>
            {
                await Task.Delay(100);
                Interlocked.Increment(ref j);
                return "x" + args;
            }),
            cache.GetOrCreate("a", "b", async (entry, args) =>
            {
                await Task.Delay(100);
                Interlocked.Increment(ref j);
                return "x" + args;
            })
            );

        results[0].Should().Be("xa");
        results[1].Should().Be("xa");
        i.Should().Be(1);
        j.Should().Be(0);
    }
    [Fact]
    public async Task CreateRunsOnlyOnce2_SimulatingLongStart()
    {
        ICustomCache cache = new CustomCache();
        int i = 0;
        int j = 0;
        var results = await Task.WhenAll(
            cache.GetOrCreate("a", "a", async (entry, args) =>
            {
                Thread.Sleep(100);
                await Task.Delay(10);
                Interlocked.Increment(ref i);
                return "x" + args;
            }),
            cache.GetOrCreate("a", "b", async (entry, args) =>
            {
                Thread.Sleep(100);
                await Task.Delay(10);
                Interlocked.Increment(ref j);
                return "x" + args;
            }),
            cache.GetOrCreate("a", "b", async (entry, args) =>
            {
                Thread.Sleep(100);
                await Task.Delay(10);
                Interlocked.Increment(ref j);
                return "x" + args;
            }),
            cache.GetOrCreate("a", "b", async (entry, args) =>
            {
                Thread.Sleep(100);
                await Task.Delay(10);
                Interlocked.Increment(ref j);
                return "x" + args;
            })
            );

        results[0].Should().Be("xa");
        results[1].Should().Be("xa");
        i.Should().Be(1);
        j.Should().Be(0);
    }
}
