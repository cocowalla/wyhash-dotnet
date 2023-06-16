using System;
using System.Runtime.CompilerServices;

namespace WyHash.Benchmarks.Safe;

/// <summary>
/// Core constants and functions used by SafeWyHash64
/// </summary>
/// <remarks>
/// This version doesn't use `unsafe`, and is used as a performance comparison against the production code
/// <seealso cref="WyCore"/>, which does use unsafe
/// </remarks>
internal static class SafeWyCore
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
    private static (ulong Hi, ulong Lo) Multiply64(ulong x, ulong y)
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

    /// <summary>
    /// Reads an unsigned 64-bit integer from a byte array.
    /// The value is constructed by combining the hi and lo bits of 2 unsigned 32-bit integers
    /// </summary>
    /// <param name="array">Array to read the 64-bit integer from</param>
    /// <param name="start">Position from which to read the 64-bit integer</param>
    /// <returns>64-bit integer read from the array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read64Swapped(byte[] array, int start)
    {
        var left = (ulong)unchecked((array[start] << 0) | (array[start + 1] << 8) | (array[start + 2] << 16) | (array[start + 3] << 24));
        var right = (ulong)unchecked((array[start + 4] << 0) | (array[start + 5] << 8) | (array[start + 6] << 16) | (array[start + 7] << 24));

        return left << 32 | right;
    }

    // Reading ulongs from a cast span gives a big performance boost over BitConverter.ToUInt64 on a byte array
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read64(ReadOnlySpan<ulong> array, int start) =>
        array[start / 8];

    // Manually building the uint using bit shifting is faster than BitConverter.ToUInt32
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read32(byte[] array, int start) =>
        (ulong)unchecked((array[start] << 0) | (array[start + 1] << 8) | (array[start + 2] << 16) | (array[start + 3] << 24));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read16(byte[] array, int start) =>
        BitConverter.ToUInt16(array, start);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read8(byte[] array, int index) =>
        array[index];
}
