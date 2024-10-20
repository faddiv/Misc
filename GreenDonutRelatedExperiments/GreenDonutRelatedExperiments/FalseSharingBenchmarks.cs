using BenchmarkDotNet.Attributes;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GreenDonutRelatedExperiments;

[ShortRunJob]
public class FalseSharingBenchmarks
{
    public int[] _values = new int[32];
    public Values2 values;
    // Cache line 64 bytes sometimes 128 bytes
    public ValueHolder[] valueHolders =
    [
        new ValueHolder(),
        new ValueHolder(),
    ];

    [GlobalSetup]
    public void Setup()
    {
        
    }

    [GlobalCleanup]
    public void Cleanup()
    {
    }

    [Benchmark]
    public void ValueHolders()
    {
        Parallel.Invoke(
            () => Increment(ref valueHolders[0]._value),
            () => Increment(ref valueHolders[1]._value));
    }

    [Benchmark]
    public void ArrayNext()
    {
        Parallel.Invoke(
            () => Increment(ref _values[0]),
            () => Increment(ref _values[1]));
    }

    [Benchmark]
    public void ArrayFar()
    {
        Parallel.Invoke(
            () => Increment(ref _values[0]),
            () => Increment(ref _values[7]));
    }

    [Benchmark]
    public void NextToEachOther()
    {
        Parallel.Invoke(
            () => Increment(ref values._value0),
            () => Increment(ref values._value1));
    }

    [Benchmark]
    public void FarAway()
    {
        Parallel.Invoke(
            () => Increment(ref values._value0),
            () => Increment(ref values._value7));
    }

    public static void Increment(ref int intValue)
    {
        for (int i = 0; i < 100_000_000; i++)
        {
            Interlocked.Increment(ref intValue);
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Values2
{
    public int _value0;
    public int _value1;
    public int _value2;
    public int _value3;
    public int _value4;
    public int _value5;
    public int _value6;
    public int _value7;
    public int _value8;
    public int _value9;
    public int _value10;
}

public class ValueHolder
{
    public int _value;

}
