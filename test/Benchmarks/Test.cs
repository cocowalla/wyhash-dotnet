using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Standart.Hash.xxHash;
using WyHash.Benchmarks.Native;

namespace WyHash.Benchmarks
{
    //[ShortRunJob]
    //[ClrJob, CoreJob]
    [CoreJob]
    [MemoryDiagnoser]
    [InliningDiagnoser]
    [RankColumn, MinColumn, MaxColumn]
    [MarkdownExporterAttribute.GitHub]
    public class Test
    {
        public const ulong Seed = 54321L;

        // Test using difference data sizes
        public const int B = 100;
        public const int KB = 1024;
        public const int MB = 1024 * KB;
        public const int GB = 1024 * MB;
        
        private byte[] data;
        
        //[Params(B, KB, MB, GB)]
        [Params(B, KB)]
        public int DataSize;
        
        [GlobalSetup]
        public void Setup()
        {
            var rand = new Random(42);

            this.data = new byte[DataSize];
            rand.NextBytes(this.data);
        }
        
        // Compare against Standart.Hash.xxHash, which is an *extremely* well optimised implementation of xxHash:
        // https://github.com/uranium62/xxHash
        [Benchmark]
        public void TestXxHash()
        {
            var result = xxHash64.ComputeHash(this.data, this.data.Length, Seed);
        }
        
        // Compare against the native xxHash build used as a comparison by Standart.Hash.xxHash:
        // https://github.com/uranium62/xxHash/tree/native/src/Standart.Hash.xxHash.Native
        [Benchmark]
        public void TestXxHashNative()
        {
            var result = NativeXXHash64.ComputeHash64(this.data, Seed);
        }

        [Benchmark]
        public void TestWyHashNative()
        {
            var result = NativeWyHash64.ComputeHash64(this.data, Seed);
        }
        
        [Benchmark(Baseline = true)]
        public void TestWyHash()
        {
            var result = WyHash64.ComputeHash64(this.data, Seed);
        }
    }
}
