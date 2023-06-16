using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;

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
public class Read8Tests
{
    private byte[] data;

    [GlobalSetup]
    public void Setup()
    {
        var rand = new Random(42);

        this.data = new byte[100];
        rand.NextBytes(this.data);
    }

    [Benchmark]
    public void TestRead8Array()
    {
        for (var i = 0; i < 50; i++)
        {
            var result = Read8Array(this.data, i);
        }
    }

    [Benchmark]
    public unsafe void TestRead8WyCore()
    {
        fixed (byte* pData = this.data)
        {
            byte* ptr = pData;

            for (var i = 0; i < 50; i++)
            {
                var result = WyCore.Read8(ptr, i);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read8Array(byte[] array, int index) =>
        array[index];
}
