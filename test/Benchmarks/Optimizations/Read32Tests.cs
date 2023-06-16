using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;
using WyHash.Benchmarks.Safe;

namespace WyHash.Benchmarks.Optimizations;

/// <summary>
/// Benchmarks optimizations for reading unsigned 32-bit integers from a byte array
/// </summary>
[ShortRunJob(RuntimeMoniker.Net60)]
//[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
[InliningDiagnoser(true, new[] { "WyHash" })]
[RankColumn, MinColumn, MaxColumn]
[MarkdownExporterAttribute.GitHub]
public class Read32Tests
{
    private byte[] data;

    [GlobalSetup]
    public void Setup()
    {
        var rand = new Random(42);

        this.data = new byte[100];
        rand.NextBytes(this.data);
    }

    [Benchmark(Baseline = true)]
    public void TestRead32BitConverter()
    {
        for (var i = 0; i < 50; i++)
        {
            var result = BitConverter.ToUInt32(this.data, i);
        }
    }

    [Benchmark]
    public void TestRead32Manual()
    {
        for (var i = 0; i < 50; i++)
        {
            var result = SafeWyCore.Read32(this.data, i);
        }
    }

    [Benchmark]
    public void TestRead64Span()
    {
        var span32 = MemoryMarshal.Cast<byte, uint>(this.data);

        for (var i = 0; i < 50; i++)
        {
            var result = Read32Span(span32, i);
        }
    }

    [Benchmark]
    public void TestRead64UnsafeAs()
    {
        for (var i = 0; i < 50; i++)
        {
            var result = Read32UnsafeAs(ref this.data[0], i);
        }
    }

    [Benchmark]
    public unsafe void TestRead32WyCore()
    {
        fixed (byte* pData = this.data)
        {
            byte* ptr = pData;

            for (var i = 0; i < 50; i++)
            {
                var result = WyCore.Read32(ptr, i);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Read32UnsafeAs(ref byte data, int start) =>
        Unsafe.As<byte, uint>(ref Unsafe.Add(ref data, start));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Read32Span(ReadOnlySpan<uint> array, int start) =>
        array[start / 4];
}
