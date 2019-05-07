``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E3-1545M v5 2.90GHz, 1 CPU, 8 logical and 4 physical cores
Frequency=2836132 Hz, Resolution=352.5929 ns, Timer=TSC
.NET Core SDK=2.2.105
  [Host] : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Core   : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT

Job=Core  Runtime=Core  

```
|           Method | DataSize |      Mean |     Error |    StdDev |    Median |       Min |       Max | Ratio | RatioSD | Rank | Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------- |--------- |----------:|----------:|----------:|----------:|----------:|----------:|------:|--------:|-----:|------:|------:|------:|----------:|
|       TestXxHash |      100 |  18.19 ns | 0.1241 ns | 0.1161 ns |  18.14 ns |  18.05 ns |  18.43 ns |  0.57 |    0.01 |    2 |     - |     - |     - |         - |
| TestXxHashNative |      100 |  21.85 ns | 0.1671 ns | 0.1563 ns |  21.90 ns |  21.63 ns |  22.08 ns |  0.69 |    0.01 |    3 |     - |     - |     - |         - |
| TestWyHashNative |      100 |  16.50 ns | 0.3931 ns | 1.1528 ns |  15.88 ns |  15.37 ns |  19.77 ns |  0.57 |    0.02 |    1 |     - |     - |     - |         - |
|     *TestWyHash* |      100 |  31.84 ns | 0.2136 ns | 0.1998 ns |  31.85 ns |  31.47 ns |  32.18 ns |  1.00 |    0.00 |    4 |     - |     - |     - |         - |
|                  |          |           |           |           |           |           |           |       |         |      |       |       |       |           |
|       TestXxHash |     1024 |  97.64 ns | 0.6791 ns | 0.6020 ns |  97.52 ns |  96.98 ns |  99.13 ns |  0.41 |    0.01 |    3 |     - |     - |     - |         - |
| TestXxHashNative |     1024 |  89.51 ns | 1.1083 ns | 1.0367 ns |  89.92 ns |  87.46 ns |  90.84 ns |  0.37 |    0.01 |    2 |     - |     - |     - |         - |
| TestWyHashNative |     1024 |  71.31 ns | 0.7103 ns | 0.6644 ns |  71.34 ns |  70.54 ns |  72.85 ns |  0.30 |    0.01 |    1 |     - |     - |     - |         - |
|     *TestWyHash* |     1024 | 238.94 ns | 5.0078 ns | 5.1426 ns | 236.56 ns | 234.14 ns | 249.36 ns |  1.00 |    0.00 |    4 |     - |     - |     - |         - |
