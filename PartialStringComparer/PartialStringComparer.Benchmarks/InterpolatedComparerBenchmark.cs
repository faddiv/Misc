using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartialStringComparer.Benchmarks;

[MemoryDiagnoser]
public class InterpolatedComparerBenchmark
{
    private string _s1 = "";
    private SomeEnum _enum;
    private int _number;

    [GlobalSetup]
    public void Setup()
    {
        _s1 = "This is TheValue: 12543";
        _enum = SomeEnum.TheValue;
        _number = 12543;
    }

    [Benchmark]
    public bool Baseline()
    {
        return Equals(_s1, $"This is {_enum}: {_number}");
    }

    [Benchmark]
    public bool Optimized()
    {
        // Still allocation on _enum and _number. Why?
        return InterpolatedComparer.Equals(_s1, $"This is {_enum}: {_number}");
    }

    public enum SomeEnum
    {
        None,
        TheValue
    }
}
