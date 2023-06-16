using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;

namespace WyHash.Benchmarks.Optimizations;

/// <summary>
/// Benchmarks optimizations for reading unsigned 64-bit integers from a byte array
/// </summary>
[ShortRunJob(RuntimeMoniker.Net60)]
//[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
[InliningDiagnoser(true, new[] { "WyHash" })]
[RankColumn, MinColumn, MaxColumn]
[MarkdownExporterAttribute.GitHub]
public class Read64Tests
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
    public void TestRead64BitConverter()
    {
        for (var i = 0; i < 50; i++)
        {
            var result = BitConverter.ToUInt64(this.data, i);
        }
    }

    [Benchmark]
    public void TestRead64UnsafeAs()
    {
        for (var i = 0; i < 50; i++)
        {
            var result = Read64UnsafeAs(ref this.data[0], i);
        }
    }

    [Benchmark]
    public void TestRead64Span()
    {
        var span64 = MemoryMarshal.Cast<byte, ulong>(this.data);

        for (var i = 0; i < 50; i++)
        {
            var result = Read64Span(span64, i);
        }
    }

    [Benchmark]
    public void TestRead64BinaryPrimitives()
    {
        for (var i = 0; i < 50; i++)
        {
            var result = BinaryPrimitives.ReadUInt64LittleEndian(this.data);
        }
    }

    [Benchmark]
    public unsafe void TestRead64WyCore()
    {
        fixed (byte* pData = this.data)
        {
            byte* ptr = pData;

            for (var i = 0; i < 50; i++)
            {
                var result = WyCore.Read64(ptr, i);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Read64UnsafeAs(ref byte data, int start) =>
        Unsafe.As<byte, ulong>(ref Unsafe.Add(ref data, start));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Read64Span(ReadOnlySpan<ulong> array, int start) =>
        array[start / 8];
}
