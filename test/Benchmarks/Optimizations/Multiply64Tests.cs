using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

#if NETCOREAPP3_0
using System.Runtime.Intrinsics.X86;
#endif

// ReSharper disable SuggestVarOrType_BuiltInTypes
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
        public void TestMethod4()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method4(X, Y + i);
            }
        }

        [Benchmark]
        public void TestMethod5()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method5(X, Y + i);
            }
        }

        [Benchmark]
        public void TestMethod6()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method6(X, Y + i);
            }
        }

        [Benchmark]
        public void TestMethod7()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method7(X, Y + i);
            }
        }

        [Benchmark]
        public void TestMethod8()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method8(X, Y + i);
            }
        }

        [Benchmark]
        public void TestMethod9()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method9(X, Y + i);
            }
        }

        [Benchmark]
        public void TestMethod10()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method10(X, Y + i);
            }
        }

        [Benchmark]
        public void TestMethod11()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method11(X, Y + i);
            }
        }

#if NETCOREAPP3_0
        [Benchmark]
        public void TestMethod12()
        {
            for (ulong i = 0; i < 100; i++)
            {
                var result = Method12(X, Y + i);
            }
        }
#endif

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

        // This is what we used in WyCore 1.0.0, but Method10 was later found to be ~9% faster
        // Similar to mulul64 from Hacker's Delight
        // https://www.codeproject.com/Tips/618570/UInt-Multiplication-Squaring
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method4(ulong x, ulong y)
        {
            var lo = x * y;

            var u1 = x & 0xffffffff;
            var v1 = y & 0xffffffff;
            var t = u1 * v1;
            var k = t >> 32;

            var xHi = x >> 32;
            t = xHi * v1 + k;
            k = t & 0xffffffff;
            var w1 = t >> 32;

            y >>= 32;
            t = u1 * y + k;
            k = t >> 32;

            var hi = xHi * y + w1 + k;

            return (hi, lo);
        }

        // https://stackoverflow.com/a/29722382/25758
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method5(ulong x, ulong y)
        {
            var lo = x * y;

            ulong accum = (uint)x * (ulong)(uint)y;
            accum >>= 32;

            ulong term1 = (x >> 32) * (uint)y;
            ulong term2 = (y >> 32) * (uint)x;

            accum += (uint)term1;
            accum += (uint)term2;
            accum >>= 32;
            accum += (term1 >> 32) + (term2 >> 32);
            accum += (x >> 32) * (y >> 32);

            return (accum, lo);
        }

        // https://github.com/cag-group/Luger/blob/6ecb9c218fb96e98f7d22942124ac194c0d57aff/src/Utilities/IntExt.cs#L31
        private const ulong LO_MASK = 0x0000_0000_FFFF_FFFF;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method6(ulong x, ulong y)
        {
            var lo = x * y;

            // x = xLo + (xHi << 32)
            // y = yLo + (yHi << 32)
            var xHi = x >> 32;
            var xLo = x & LO_MASK;
            var yHi = y >> 32;
            var yLo = y & LO_MASK;

            // x * y >> 64 = (xLo + (xHi << 32)) * (yLo + (yHi << 32)) >> 64
            //             = xLo * yLo + xLo * (yHi << 32) + (xHi << 32) * yl + (xHi << 32) * (yHi << 32) >> 64
            // Can not overflow
            var acc1 = (xLo * yLo >> 32) + xLo * yHi;

            var xhyl = xHi * yLo;

            // Can overflow
            var acc2 = acc1 + xhyl;

            var carry = (acc1 ^ ((acc1 ^ xhyl) & (xhyl ^ acc2))) >> 31 & ~LO_MASK;

            // Can not overflow
            var hi = (acc2 >> 32) + carry + xHi * yHi;

            return (hi, lo);
        }

        // This is the method used in the original C source when intrinsics are not available
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method7(ulong x, ulong y)
        {
            ulong ha = x >> 32;
            ulong hb = y >> 32;
            ulong la = (uint)x;
            ulong lb = (uint)y;

            ulong rh = ha * hb;
            ulong rm0 = ha * lb;
            ulong rm1 = hb * la;
            ulong rl = la * lb;
            ulong t = rl + (rm0 << 32);

            ulong c = t < rl ? 1ul : 0ul;

            ulong lo = t + (rm1 << 32);

            if (lo < t)
                c++;

            ulong hi = rh + (rm0 >> 32) + (rm1 >> 32) + c;

            return (hi, lo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong umulhi(ulong a, ulong b)
        {
            var c = a * b;
            return c >> 32;
        }

        // https://github.com/wokaka/cedp/blob/effa465145635fd20bc16ef2e8d6e91d54cea505/projects/gpgpu/benchmark/parboil/benchmarks/sad/largerBlocks.cu.cpp#L7652
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method8(ulong a, ulong b)
        {
            var lo = a * b;

            var aLo = (uint)a;
            var aHi = a >> 32;
            var bLo = (uint)b;
            var bHi = b >> 32;

            var mid1 = aLo * bHi;
            var mid2 = aHi * bLo;

            var carry = (umulhi(aLo, bLo) + (uint)mid1 + (uint)mid2) >> 32;

            var hi = aHi * bHi + (mid1 >> 32) + (mid2 >> 32) + carry;

            return (hi, lo);
        }

        // Similar to mulul64 from Hacker's Delight
        // https://github.com/rahulmula/Gladiator_Hip/blob/fcede25343a79f5005f5a4d88a8c0e41205dc08f/src/device_functions.cpp#L313
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method9(ulong x, ulong y)
        {
            var lo = x * y;

            var x0 = x & 0xffffffffUL;
            var x1 = x >> 32;
            var y0 = y & 0xffffffffUL;
            var y1 = y >> 32;

            var z0 = x0 * y0;
            var t = x1 * y0 + (z0 >> 32);
            var z1 = t & 0xffffffffUL;
            var z2 = t >> 32;
            z1 = x0 * y1 + z1;

            var hi = x1 * y1 + z2 + (z1 >> 32);

            return (hi, lo);
        }

        // This is what we use in WyCore
        // https://stackoverflow.com/a/51587262/25758, but with a faster lo calculation
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method10(ulong x, ulong y)
        {
            var lo = x * y;

            ulong x0 = (uint)x;
            ulong x1 = x >> 32;

            ulong y0 = (uint)y;
            ulong y1 = y >> 32;

            ulong p11 = x1 * y1;
            ulong p01 = x0 * y1;
            ulong p10 = x1 * y0;
            ulong p00 = x0 * y0;

            // 64-bit product + two 32-bit values
            ulong middle = p10 + (p00 >> 32) + (uint)p01;

            // 64-bit product + two 32-bit values
            ulong hi = p11 + (middle >> 32) + (p01 >> 32);

            return (hi, lo);
        }

        // mulul64 from Hacker's Delight, but with a faster lo calculation
        // https://www.hackersdelight.org/hdcodetxt/mont64.c.txt
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Method11(ulong u, ulong v)
        {
            var lo = u * v;

            var u1 = u >> 32;
            var u0 = u & 0xFFFFFFFF;
            var v1 = v >> 32;
            var v0 = v & 0xFFFFFFFF;

            var t = u0 * v0;
            var k = t >> 32;

            t = u1 * v0 + k;
            var w1 = t & 0xFFFFFFFF;
            var w2 = t >> 32;

            t = u0 * v1 + w1;
            k = t >> 32;

            var hi = u1 * v1 + w2 + k;

            return (hi, lo);
        }

#if NETCOREAPP3_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe (ulong Hi, ulong Lo) Method12(ulong x, ulong y)
        {
            if (!Bmi2.X64.IsSupported)
                throw new NotSupportedException("No support for BMI2 :(");

            ulong lo;
            var hi = Bmi2.X64.MultiplyNoFlags(x, y, &lo);

            return (hi, lo);
        }
#endif
    }
}
