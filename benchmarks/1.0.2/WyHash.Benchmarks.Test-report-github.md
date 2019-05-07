``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E3-1545M v5 2.90GHz, 1 CPU, 8 logical and 4 physical cores
Frequency=2836064 Hz, Resolution=352.6014 ns, Timer=TSC
.NET Core SDK=3.0.100-preview4-011223
  [Host] : .NET Core 3.0.0-preview4-27615-11 (CoreCLR 4.6.27615.73, CoreFX 4.700.19.21213), 64bit RyuJIT
  Core   : .NET Core 3.0.0-preview4-27615-11 (CoreCLR 4.6.27615.73, CoreFX 4.700.19.21213), 64bit RyuJIT

Job=Core  Runtime=Core  

```
|           Method | DataSize |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Rank | Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------- |--------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-----:|------:|------:|------:|----------:|
|       TestXxHash |      100 |  20.84 ns | 0.4168 ns | 0.6964 ns |  20.02 ns |  22.99 ns |  0.95 |    0.03 |    2 |     - |     - |     - |         - |
| TestXxHashNative |      100 |  24.33 ns | 0.3217 ns | 0.3009 ns |  23.99 ns |  24.92 ns |  1.10 |    0.02 |    4 |     - |     - |     - |         - |
| TestWyHashNative |      100 |  18.63 ns | 0.1110 ns | 0.0984 ns |  18.47 ns |  18.87 ns |  0.84 |    0.01 |    1 |     - |     - |     - |         - |
|     *TestWyHash* |      100 |  22.18 ns | 0.2429 ns | 0.2273 ns |  21.84 ns |  22.52 ns |  1.00 |    0.00 |    3 |     - |     - |     - |         - |
|                  |          |           |           |           |           |           |       |         |      |       |       |       |           |
|       TestXxHash |     1024 | 106.06 ns | 1.0322 ns | 0.9150 ns | 105.25 ns | 108.04 ns |  0.79 |    0.01 |    3 |     - |     - |     - |         - |
| TestXxHashNative |     1024 |  93.75 ns | 1.9055 ns | 2.1943 ns |  91.79 ns |  99.02 ns |  0.71 |    0.02 |    2 |     - |     - |     - |         - |
| TestWyHashNative |     1024 |  75.38 ns | 1.1722 ns | 0.9788 ns |  74.37 ns |  77.46 ns |  0.56 |    0.01 |    1 |     - |     - |     - |         - |
|     *TestWyHash* |     1024 | 133.58 ns | 2.0582 ns | 1.9252 ns | 131.38 ns | 137.66 ns |  1.00 |    0.00 |    4 |     - |     - |     - |         - |
