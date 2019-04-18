using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace WyHash.Benchmarks.Optimizations
{
    /// <summary>
    /// Benchmarks optimizations for multiplying 2 unsigned 64-bit integers
    ///
    /// Note that this is the only real bottleneck in this implementation, as .NET doesn't support intrinsics (which
    /// would allow 64x64 multiplication in a single instruction on supported platforms).
    /// </summary>
    //[ClrJob, CoreJob]
    [ShortRunJob]
    [MemoryDiagnoser]
    [InliningDiagnoser]
    [RankColumn, MinColumn, MaxColumn]
    [MarkdownExporterAttribute.GitHub]
    public class Multiply64Tests
    {
        private const ulong X = Int64.MaxValue / 10;
        private const ulong Y = Int64.MaxValue / 100;
        
        [Benchmark]
        public void TestMethod1()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method1(X, Y + i);
            }
        }
        
        [Benchmark]
        public void TestMethod2()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method2(X, Y + i);
            }
        }
        
        [Benchmark]
        public void TestMethod3()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method3(X, Y + i);
            }
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method1(ulong x, ulong y)
        {
            var xHi = x >> 32;
            var xLo = x & 0xFFFFFFFF;
            var bHi = y >> 32;
            var bLo = y & 0xFFFFFFFF;

            var product1 = xLo * bLo;
            var product2 = xHi * bLo;
            var product3 = xLo * bHi;
            var product4 = xHi * bHi;

            var carry = (uint)(((product1 >> 32) + (uint)product2 + (uint)product3) >> 32);
            var tmp = (product2 >> 32) + (product3 >> 32);

            var hi = product4 + tmp + carry;
            var lo = product1 + (product2 << 32) + (product3 << 32);

            return (hi, lo);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method2(ulong x, ulong y)
        {
            var xHi = x >> 32;
            var xLo = x & 0xFFFFFFFF;
            var bHi = y >> 32;
            var bLo = y & 0xFFFFFFFF;
            
            var hi = xHi * bHi;
            var lo = xLo * bLo;
            
            var addLow = xHi * bLo + xLo * bHi;
            var addHigh = addLow >> 32;
            addLow <<= 32;
            
            var c = ((lo & addLow & 1) + (lo >> 1) + (addLow >> 1)) >> 63;
            hi += addHigh + c;
            lo += addLow;
            
            return (hi, lo);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method3(ulong x, ulong y)
        {
            var u1 = x & 0xffffffff;
            var v1 = y & 0xffffffff;
            var t = u1 * v1;
            var w3 = t & 0xffffffff;
            var k = t >> 32;

            x >>= 32;
            t = x * v1 + k;
            k = t & 0xffffffff;
            var w1 = t >> 32;

            y >>= 32;
            t = u1 * y + k;
            k = t >> 32;

            var hi = x * y + w1 + k;
            var lo = (t << 32) + w3;
            
            return (hi, lo);
        }
    }
}