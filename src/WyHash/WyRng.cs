using System;
using System.Runtime.CompilerServices;

// ReSharper disable SuggestVarOrType_BuiltInTypes
namespace WyHash
{
    /// <summary>
    /// .NET implementation of the wyrand PRNG by Wang Yi. Reference implementation (version 20190328):
    /// https://github.com/wangyi-fudan/wyhash/blob/master/wyhash.h
    /// </summary>
    public class WyRng
    {
        private ulong seed;

        /// <summary>
        /// Initializes a new instance with the specified seed value
        /// </summary>
        /// <param name="seed">Initial seed</param>
        public WyRng(ulong seed)
        {
            this.seed = seed;
        }

        /// <summary>
        /// Returns a non-negative, random 64-bit integer
        /// </summary>
        /// <returns>A non-negative, random 64-bit integer</returns>
        public ulong NextLong()
        {
            this.seed += WyCore.Prime0;
            var result = WyCore.Mum(this.seed ^ WyCore.Prime1, this.seed);

            return result;
        }

        /// <summary>
        /// Returns a non-negative, random integer
        /// </summary>
        /// <returns>A non-negative, random integer</returns>
        public int Next()
        {
            var next = NextLong();
            return (int)next;
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random bytes
        /// </summary>
        /// <param name="buffer">Array to fill with random bytes</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // Determine how many 64-bit integers we need to generate to fill the buffer
            var numVals = Math.Ceiling((double)buffer.Length / 8);

            // Fill the buffer with 8 bytes (1 ulong) at a time (WriteBytes will ensure we correctly handle any
            // remaining bytes)
            for (int i = 0; i < numVals; i++)
            {
                var next = NextLong();
                WriteBytes(next, buffer, i * 8);
            }
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random bytes
        /// </summary>
        /// <param name="buffer">Array to fill with random bytes</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextBytes(Span<byte> buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // Determine how many 64-bit integers we need to generate to fill the buffer
            var numVals = Math.Ceiling((double)buffer.Length / 8);

            // Fill the buffer with 8 bytes (1 ulong) at a time (WriteBytes will ensure we correctly handle any
            // remaining bytes)
            for (int i = 0; i < numVals; i++)
            {
                var next = NextLong();
                WriteBytes(next, buffer, i * 8);
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)NextLong();
            }
        }

        /// <summary>
        /// Writes an unsigned 64-bit integer to an existing byte array
        /// </summary>
        /// <param name="value">64-bit integer to be written</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset at which to write the 64-bit integer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteBytes(ulong value, Span<byte> array, int offset)
        {
            var max = Math.Min(8, array.Length - offset);

            for (int i = 0; i < max; i++)
            {
                array[offset + i] = (byte)value;
                value >>= 8;
            }
        }
    }
}
