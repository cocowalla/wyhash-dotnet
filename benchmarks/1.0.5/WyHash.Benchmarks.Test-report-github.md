``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19045.3086/22H2/2022Update)
AMD Ryzen 5 2600, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.107
  [Host]            : .NET 6.0.18 (6.0.1823.26907), X64 RyuJIT AVX2
  ShortRun-.NET 6.0 : .NET 6.0.18 (6.0.1823.26907), X64 RyuJIT AVX2

Job=ShortRun-.NET 6.0  Runtime=.NET 6.0  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
|           Method | DataSize |      Mean |     Error |   StdDev |       Min |       Max | Ratio | Rank | Allocated | Alloc Ratio |
|----------------- |--------- |----------:|----------:|---------:|----------:|----------:|------:|-----:|----------:|------------:|
|       TestXxHash |      100 |  17.77 ns |  1.179 ns | 0.065 ns |  17.71 ns |  17.84 ns |  0.97 |    2 |         - |          NA |
| TestXxHashNative |      100 |  21.89 ns |  4.087 ns | 0.224 ns |  21.75 ns |  22.14 ns |  1.19 |    4 |         - |          NA |
| TestWyHashNative |      100 |  14.58 ns |  1.336 ns | 0.073 ns |  14.53 ns |  14.67 ns |  0.79 |    1 |         - |          NA |
|       **TestWyHash** |      **100** |  **18.40 ns** |  **2.422 ns** | **0.133 ns** |  **18.30 ns** |  **18.55 ns** |  **1.00** |    **3** |         **-** |          **NA** |
|                  |          |           |           |          |           |           |       |      |           |             |
|       TestXxHash |     1024 |  89.38 ns | 10.387 ns | 0.569 ns |  88.95 ns |  90.02 ns |  0.73 |    3 |         - |          NA |
| TestXxHashNative |     1024 |  81.17 ns |  3.474 ns | 0.190 ns |  80.98 ns |  81.36 ns |  0.67 |    2 |         - |          NA |
| TestWyHashNative |     1024 |  64.38 ns |  2.001 ns | 0.110 ns |  64.27 ns |  64.49 ns |  0.53 |    1 |         - |          NA |
|       **TestWyHash** |     **1024** | **121.77 ns** | **23.792 ns** | **1.304 ns** | **120.53 ns** | **123.13 ns** |  **1.00** |    **4** |         **-** |          NA |
