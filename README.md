wyhash-dotnet
=============
[![NuGet](https://img.shields.io/nuget/v/WyHash.svg)](https://www.nuget.org/packages/WyHash)
[![Build Status](https://ci.appveyor.com/api/projects/status/yv41pshy1xaks5ps?svg=true)](https://ci.appveyor.com/project/cocowalla/wyhash-dotnet)

Zero-allocation C# implementation of [Wang Yi's](https://github.com/wangyi-fudan/wyhash) 64-bit **wyhash** hash algorithm and **wyrand** PRNG.

wyhash is an extremely fast, portable hashing algorithm, and passes all of the [SMHasher](https://github.com/rurban/smhasher) tests (which evaluates collision, dispersion and randomness qualities of hash functions).

Note wyhash-dotnet currently implements wyhash v1. v2 will be implemented once considered "stable".

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
Note wyhash-dotnet currently implements wyhash v1. v2 will be implemented once considered "stable".

Future improvements include support for `Span` and incremental hashing (useful for hashing streams).

At present (July 2019), wyhash is the fastest algorithm in the [SMHasher](https://github.com/rurban/smhasher) benchmark.

On a dev laptop with 64GB RAM and an Intel Xeon CPU E3-1545M v5 2.90GHz CPU, this implementation can process data at a rate of around 5.5GB/s on .NET Core 3, or 3.3GB/s on .NET Core 2 or the .NET Framework - this is *very* fast.

The reason for the performance improvement on .NET Core 3+ is that it supports [hardware intrinsics](https://fiigii.com/2019/03/03/Hardware-intrinsic-in-NET-Core-3-0-Introduction/), and wyhash-dotnet uses the [BMI2 `MULX`](https://www.felixcloutier.com/x86/MULX.html) instruction to achieve faster 64-bit integer multiplication (on systems where BMI2 is available). Support for intrinsics won't make it into the .NET Framework, but will also be in the newly announced .NET 5.

Note that `PInvoke`ing into a native DLL built using the reference C code (see the `WyHash.Native` project) achieves around 10.8GB/s (more or less *RAM SPEED*), so there is still work to do to bridge the performance gap between C# and native - I'm very much open to suggestions here!

Latest benchmarks (`DataSize` is the size of data hashed, in bytes):

```ini

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
