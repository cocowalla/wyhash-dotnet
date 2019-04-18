wyhash-dotnet
=============
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

On a dev laptop with 64GB RAM and an Intel Xeon CPU E3-1545M v5 2.90GHz CPU, this implementation can process data at a rate of around 3GB/s, which is *very* fast.

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

```
|           Method | DataSize |      Mean |     Error |    StdDev |    Median |       Min |       Max | Ratio | RatioSD | Rank | Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------- |--------- |----------:|----------:|----------:|----------:|----------:|----------:|------:|--------:|-----:|------:|------:|------:|----------:|
|       TestXxHash |      100 |  20.40 ns | 0.4396 ns | 0.3671 ns |  20.48 ns |  19.71 ns |  20.92 ns |  0.48 |    0.02 |    2 |     - |     - |     - |         - |
| TestXxHashNative |      100 |  24.53 ns | 0.5409 ns | 0.5313 ns |  24.43 ns |  23.95 ns |  25.71 ns |  0.58 |    0.02 |    3 |     - |     - |     - |         - |
| TestWyHashNative |      100 |  18.96 ns | 0.0906 ns | 0.0757 ns |  18.95 ns |  18.85 ns |  19.12 ns |  0.44 |    0.01 |    1 |     - |     - |     - |         - |
|     *TestWyHash* |      100 |  40.53 ns | 0.8427 ns | 1.6829 ns |  39.59 ns |  39.13 ns |  44.56 ns |  1.00 |    0.00 |    4 |     - |     - |     - |         - |
|                  |          |           |           |           |           |           |           |       |         |      |       |       |       |           |
|       TestXxHash |     1024 |  99.90 ns | 0.7521 ns | 0.6667 ns |  99.74 ns |  99.06 ns | 101.42 ns |  0.32 |    0.01 |    3 |     - |     - |     - |         - |
| TestXxHashNative |     1024 |  98.04 ns | 2.2641 ns | 3.2471 ns |  96.98 ns |  93.90 ns | 105.07 ns |  0.31 |    0.01 |    2 |     - |     - |     - |         - |
| TestWyHashNative |     1024 |  78.13 ns | 0.6726 ns | 0.5963 ns |  78.04 ns |  77.34 ns |  79.58 ns |  0.25 |    0.01 |    1 |     - |     - |     - |         - |
|     *TestWyHash* |     1024 | 313.00 ns | 6.1691 ns | 8.6482 ns | 310.51 ns | 302.29 ns | 328.01 ns |  1.00 |    0.00 |    4 |     - |     - |     - |         - |
