```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7462/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 2600 3.40GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3
  Job-RYMNDB : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3

OutlierMode=RemoveAll  Runtime=.NET 8.0  

```
| Method                    | Mean      | Error     | StdDev    | Rank | Gen0   | Allocated |
|-------------------------- |----------:|----------:|----------:|-----:|-------:|----------:|
| DecorateSimple            |  3.619 ns | 0.0657 ns | 0.0615 ns |    1 |      - |         - |
| WithDispatchProxy         | 57.921 ns | 0.8224 ns | 0.7693 ns |    6 | 0.0267 |     112 B |
| WithCompositeDynamicProxy | 55.091 ns | 0.2766 ns | 0.2310 ns |    5 | 0.0459 |     192 B |
| WithInheritedDynamicProxy | 48.373 ns | 0.2579 ns | 0.2154 ns |    4 | 0.0459 |     192 B |
| WithLightInject           | 43.791 ns | 0.8511 ns | 0.7961 ns |    3 | 0.0363 |     152 B |
| WithExperimental          | 16.644 ns | 0.1875 ns | 0.1662 ns |    2 |      - |         - |
