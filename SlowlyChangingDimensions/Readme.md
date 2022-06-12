# What does this do?
Test slowly changing dimensions usecases
First idea: Lets be only one date and the next row references to the previous one. So this way we doesnt need "valid from" and "valid to" dates.
Second idea: The classic "valid from" and "valid to". Since we query one table and no left join, it is faster.
Last test case: Instead using datetime now to select the current version use a current version boolean switch. This seems faster.

Benchmark results:
|                Method |      Mean |    Error |   StdDev |    Median | Ratio | RatioSD | Allocated |
|---------------------- |----------:|---------:|---------:|----------:|------:|--------:|----------:|
|   ScdWithLeftJoinPast | 128.82 ms | 2.463 ms | 2.184 ms | 128.62 ms |  1.00 |    0.00 |      2 MB |
|   ScdWithLeftJoinLast | 177.87 ms | 3.507 ms | 6.500 ms | 176.95 ms |  1.40 |    0.06 |      2 MB |
|     ScdWithFromToPast |  17.51 ms | 0.332 ms | 0.757 ms |  17.22 ms |  0.14 |    0.01 |      2 MB |
|     ScdWithFromToLast |  30.83 ms | 0.593 ms | 0.906 ms |  30.45 ms |  0.24 |    0.01 |      2 MB |
| ScdWithCurrentVersion |  16.52 ms | 0.147 ms | 0.196 ms |  16.45 ms |  0.13 |    0.00 |      2 MB |