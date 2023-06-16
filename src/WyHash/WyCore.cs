using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace WyHash;

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
    ///
    /// Source: https://stackoverflow.com/a/51587262/25758, but with a faster lo calculation
    /// </summary>
    /// <remarks>
    /// <seealso cref="System.Numerics.BigInteger"/> can perform multiplication on large integers, but it's
    /// comparatively slow, and an equivalent method allocates around 360B/call
    /// </remarks>
    /// <param name="x">First 64-bit integer</param>
    /// <param name="y">Second 64-bit integer</param>
    /// <returns>Product of <paramref name="x"/> and <paramref name="y"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("ReSharper", "JoinDeclarationAndInitializer")]
    internal static unsafe (ulong Hi, ulong Lo) Multiply64(ulong x, ulong y)
    {
        ulong hi;
        ulong lo;

// Use BMI2 intrinsics where available
#if NETCOREAPP3_0_OR_GREATER
            if (System.Runtime.Intrinsics.X86.Bmi2.X64.IsSupported)
            {
                hi = System.Runtime.Intrinsics.X86.Bmi2.X64.MultiplyNoFlags(x, y, &lo);
                return (hi, lo);
            }
#endif

        lo = x * y;

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
        hi = p11 + (middle >> 32) + (p01 >> 32);

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
