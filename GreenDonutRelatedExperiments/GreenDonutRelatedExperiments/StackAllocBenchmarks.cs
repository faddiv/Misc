using BenchmarkDotNet.Attributes;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace GreenDonutRelatedExperiments;

[MemoryDiagnoser]
public class StackAllocBenchmarks
{
    public List<string> texts = [];
    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < 16; i++) {
            texts.Add($"item{i}");
        }
    }

    [Benchmark]
    public int StackAlloc()
    {
        Span<string> span;
        string[]? array = null;
        var count = texts.Count;
        if (count is 0)
        {
            return 0;
        }
        else if (count > 16)
        {
            array = ArrayPool<string>.Shared.Rent(count);
            span = array.AsSpan(0, count);
        }
        else
        {
            var first = texts[0];
            var data = new MyData(first);
            span = MemoryMarshal.CreateSpan(ref data.first, count);
        }
        texts.CopyTo(span);
        var sum = 0;
        foreach (var item in span)
        {
            sum += item.Length;
        }
        if (array is not null)
        {
            ArrayPool<string>.Shared.Return(array);
        }
        return sum;
    }

    [Benchmark]
    public int ArrayPool()
    {
        Span<string> span;
        string[]? array;
        var count = texts.Count;
        if (count is 0)
        {
            return 0;
        }

        array = ArrayPool<string>.Shared.Rent(count);
        span = array.AsSpan(0, count);

        texts.CopyTo(span);
        var sum = 0;
        foreach (var item in span)
        {
            sum += item.Length;
        }
        if (array is not null)
        {
            ArrayPool<string>.Shared.Return(array);
        }
        return sum;
    }

    [Benchmark]
    public int ArrayCopy()
    {
        Span<string> span;
        string[]? array;
        var count = texts.Count;
        if (count is 0)
        {
            return 0;
        }

        array = [.. texts];
        span = array.AsSpan();

        var sum = 0;
        foreach (var item in span)
        {
            sum += item.Length;
        }

        return sum;
    }
}

[InlineArray(16)]
public struct MyData(string f)
{
    public string first = f;
}
// 15.83 ns 15.86 ns 15.86 ns
// 37.27 ns 37.31 ns 37.45 ns
