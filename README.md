wyhash-dotnet
=============
[![NuGet](https://img.shields.io/nuget/v/WyHash.svg)](https://www.nuget.org/packages/WyHash)

Zero-allocation C# implementation of [Wang Yi's](https://github.com/wangyi-fudan/wyhash) 64-bit **wyhash** hash algorithm and **wyrand** PRNG.

wyhash is an extremely fast, portable hashing algorithm, and passes all of the [SMHasher](https://github.com/rurban/smhasher) tests (which evaluates collision, dispersion and randomness qualities of hash functions).

Getting Started
-----

Install the [WyHash](https://www.nuget.org/packages/WyHash) package from NuGet:

```powershell
Install-Package WyHash
```

WyHash implements [HashAlgorithm](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm?view=netframework-4.7.2), so can be integrated into existing projects easily:

```csharp
var seed = 42;
var hasher = WyHash64.Create(seed);

var result = hasher.ComputeHash(myData);
```

A common use case is getting the resulting hash as a 64-bit unsigned integer (`ulong`), so a static convenience method is also provided:

```csharp
// Note that if no seed is specified, it default to 0
var seed = 42;
var result = WyHash64.ComputeHash64(myData, seed);
```

The wyrand PRNG is also implemented:

```csharp
var rng = new WyRng(42);

// Generate pseudorandom values
ulong a64BitVal = rng.NextLong();
uint a32BitVal = rng.Next();

// Fill a buffer with pseudorandom bytes
var buffer = new byte[256];
rng.NextBytes(buffer);

// Span is also supported
Span<byte> spanBuffer = stackalloc byte[128];
rng.NextBytes(spanBuffer);
```

Performance & Future Work
-------------------------
At present (April 2019), wyhash is the fastest algorithm in the [SMHasher](https://github.com/rurban/smhasher) benchmark.

On a dev laptop with 64GB RAM and an Intel Xeon CPU E3-1545M v5 2.90GHz CPU, this implementation can process data at a rate of around 3.3GB/s, which is *very* fast.

Note that `PInvoke`ing into a native DLL built using the reference C code (see the `WyHash.Native` project) achieves around 10.8GB/s, so there is still work to do to bridge the performance gap.

The bottleneck is **64-bit integer multiplication**, as .NET doesn't yet support intrinsics, which would allow performing 64x64 multiplication in a single instruction on supported platforms. Work on this is ongoing by the .NET team, and is expected to land in .NET Core 3.0 - support is unlikely to make it into the .NET Framework though. I expected that using instructions such as BMI2 `MULX` or SSE4.1 `PMULLD` will get this implementation running at roughly the same speeds as the C reference code (that is, more or less *RAM SPEED*)

Latest benchmarks (`DataSize` is the size of data hashed, in bytes):

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E3-1545M v5 2.90GHz, 1 CPU, 8 logical and 4 physical cores
Frequency=2836132 Hz, Resolution=352.5929 ns, Timer=TSC
.NET Core SDK=2.2.105
  [Host] : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Core   : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT

Job=Core  Runtime=Core  

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
