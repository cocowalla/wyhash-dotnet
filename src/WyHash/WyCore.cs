using System.Runtime.CompilerServices;

namespace WyHash
{
    /// <summary>
    /// Core constants and functions used by both WyRng and WyHash64
    /// </summary>
    internal static class WyCore
    {
        internal const ulong Prime0 = 0xa0761d6478bd642f;
        internal const ulong Prime1 = 0xe7037ed1a0b428db;
        internal const ulong Prime2 = 0x8ebc6af09c88c6e3;
        internal const ulong Prime3 = 0x589965cc75374cc3;
        internal const ulong Prime4 = 0x1d8e4e27c47d124f;
        internal const ulong Prime5 = 0xeb44accab455d165;

        /// <summary>
        /// Perform a MUM (MUltiply and Mix) operation. Multiplies 2 unsigned 64-bit integers, then combines the
        /// hi and lo bits of the resulting 128-bit integer using XOR 
        /// </summary>
        /// <param name="x">First 64-bit integer</param>
        /// <param name="y">Second 64-bit integer</param>
        /// <returns>Result of the MUM (MUltiply and Mix) operation</returns>
        internal static ulong Mum(ulong x, ulong y)
        {
            var (hi, lo) = Multiply64(x, y);
            return hi ^ lo;
        }
        
        /// <summary>
        /// Multiplies 2 unsigned 64-bit integers, returning the result in 2 ulongs representing the hi and lo bits
        /// of the resulting 128-bit integer
        /// </summary>
        /// <remarks>
        /// <seealso cref="System.Numerics.BigInteger"/> can perform multiplication on large integers, but it's
        /// comparatively slow, and an equivalent method allocates around 360B/call
        /// </remarks>
        /// <param name="x">First 64-bit integer</param>
        /// <param name="y">Second 64-bit integer</param>
        /// <returns>Product of <paramref name="x"/> and <paramref name="y"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (ulong Hi, ulong Lo) Multiply64(ulong x, ulong y)
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

        /// <summary>
        /// Reads an unsigned 64-bit integer from a byte array.
        /// The value is constructed by combining the hi and lo bits of 2 unsigned 32-bit integers
        /// </summary>
        /// <param name="ptr">Pointer to a byte array from which to read the 64-bit integer from</param>
        /// <param name="start">Position from which to read the 64-bit integer</param>
        /// <returns>64-bit integer read from the byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ulong Read64Swapped(byte* ptr, int start)
        {
            var left = (ulong)*(uint*)(ptr + start);
            var right = (ulong)*(uint*)(ptr + start + 4);

            return left << 32 | right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ulong Read64(byte* ptr, int start) =>
            *(ulong*)(ptr + start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ulong Read32(byte* ptr, int start) =>
            *(uint*)(ptr + start);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ulong Read16(byte* ptr, int start) =>
            *(ushort*)(ptr + start);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ulong Read8(byte* ptr, int index) =>
            *(ptr + index);
    }
}
