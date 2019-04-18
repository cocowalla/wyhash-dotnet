using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace WyHash.Benchmarks.Optimizations
{
    //[ClrJob, CoreJob]
    [ShortRunJob]
    [MemoryDiagnoser]
    [InliningDiagnoser]
    [RankColumn, MinColumn, MaxColumn]
    [MarkdownExporterAttribute.GitHub]
    public class Read64SwappedTests
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
        public void TestRead64SwappedBitConverter()
        {
            for (var i = 0; i < 50; i++)
            {
                var result = ((ulong)BitConverter.ToUInt32(this.data, i) << 32) | BitConverter.ToUInt32(this.data, i + 4);
            }
        }
        
        [Benchmark]
        public unsafe void TestRead64SwappedWyCore()
        {
            fixed (byte* pData = this.data)
            {
                byte* ptr = pData;
                
                for (var i = 0; i < 50; i++)
                {
                    var result = WyCore.Read64Swapped(ptr, i);
                }
            }
        }
    }
}
