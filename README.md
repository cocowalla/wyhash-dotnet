wyhash-dotnet
=============
[![NuGet](https://img.shields.io/nuget/v/WyHash.svg)](https://www.nuget.org/packages/WyHash)
[![Build Status](https://ci.appveyor.com/api/projects/status/yv41pshy1xaks5ps?svg=true)](https://ci.appveyor.com/project/cocowalla/wyhash-dotnet)

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
At present (May 2019), wyhash is the fastest algorithm in the [SMHasher](https://github.com/rurban/smhasher) benchmark.

On a dev laptop with 64GB RAM and an Intel Xeon CPU E3-1545M v5 2.90GHz CPU, this implementation can process data at a rate of around 5.5GB/s on .NET Core 3, or 3.3GB/s on .NET Core 2 or the .NET Framework - this is *very* fast.

The reason for the performance improvement on .NET Core 3 is that it supports [hardware intrinsics](https://fiigii.com/2019/03/03/Hardware-intrinsic-in-NET-Core-3-0-Introduction/), and wyhash-dotnet uses the [BMI2 `MULX`](https://www.felixcloutier.com/x86/MULX.html) instruction to achieve faster 64-bit integer multiplication (on systems where BMI2 is available). Support for intrinsics won't make it into the .NET Framework, but will also be in the newly announced .NET 5.

Note that `PInvoke`ing into a native DLL built using the reference C code (see the `WyHash.Native` project) achieves around 10.8GB/s (more or less *RAM SPEED*), so there is still work to do to bridge the performance gap between C# and native - I'm very much open to suggestions here!

Latest benchmarks (`DataSize` is the size of data hashed, in bytes):

```ini

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
