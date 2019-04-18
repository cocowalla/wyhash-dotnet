using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

// ReSharper disable SuggestVarOrType_BuiltInTypes
namespace WyHash.Benchmarks.Safe
{
    /// <inheritdoc />
    /// <summary>
    /// .NET implementation of the wyhash 64-bit hash algorithm by Wang Yi. Reference implementation (version 20190328):
    /// https://github.com/wangyi-fudan/wyhash/blob/master/wyhash.h
    /// </summary>
    /// <remarks>
    /// This version doesn't use `unsafe`, and is used as a performance comparison against the optimised production code
    /// <seealso cref="WyHash64"/>, which does use unsafe
    /// </remarks>
    public class SafeWyHash64 : HashAlgorithm
    {
        private ulong seed;
        private ulong length;
        
        /// <inheritdoc cref="HashAlgorithm.HashSize"/>    
        public override int HashSize => 64;

        public new static SafeWyHash64 Create() => new SafeWyHash64();
        public static SafeWyHash64 Create(ulong seed) => new SafeWyHash64(seed);

        private SafeWyHash64() { }
        
        private SafeWyHash64(ulong seed = 0)
        {
            this.seed = seed;
        }
        
        /// <summary>
        /// Convenience method to compute a WyHash hash and return the result as a 64-bit unsigned integer
        /// </summary>
        public static ulong ComputeHash64(byte[] array, ulong seed)
        {
            seed = WyHashCore(array, 0, array.Length, seed);
            return HashFinal(seed, (ulong)array.Length);
        }
                            
        public override void Initialize()
        {
            this.seed = 0;
            this.length = 0;
        }
        
        /// <inheritdoc />    
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            var len = cbSize - ibStart;
            this.length += (ulong)len;

            this.seed = WyHashCore(array, ibStart, cbSize, this.seed);
        }

        private static ulong WyHashCore(byte[] array, int ibStart, int cbSize, ulong seed)
        {
            // We work a lot with 64-bit integers - reading ulongs from the source array using a cast span gives a big performance boost over BitConverter.ToUInt64
            var span64 = MemoryMarshal.Cast<byte, ulong>(array);
            
            var len = cbSize - ibStart;
            var p = 0;

            for (int i = ibStart; i + 32 <= len; i += 32, p += 32)
            {
                seed = SafeWyCore.Mum(seed ^ SafeWyCore.Prime0, SafeWyCore.Mum(SafeWyCore.Read64(span64, p) ^ SafeWyCore.Prime1, SafeWyCore.Read64(span64, p + 8) ^ SafeWyCore.Prime2) ^ SafeWyCore.Mum(SafeWyCore.Read64(span64, p + 16) ^ SafeWyCore.Prime3, SafeWyCore.Read64(span64, p + 24) ^ SafeWyCore.Prime4));
            }

            seed ^= SafeWyCore.Prime0;

            switch (len & 31)
            {
                case 1:
                    seed = SafeWyCore.Mum(seed, SafeWyCore.Read8(array, p) ^ SafeWyCore.Prime1);
                    break;
                case 2:
                    seed = SafeWyCore.Mum(seed, SafeWyCore.Read16(array, p) ^ SafeWyCore.Prime1);
                    break;
                case 3:
                    seed = SafeWyCore.Mum(seed, ((SafeWyCore.Read16(array, p) << 8) | SafeWyCore.Read8(array, p + 2)) ^ SafeWyCore.Prime1);
                    break;
                case 4:
                    seed = SafeWyCore.Mum(seed, SafeWyCore.Read32(array, p) ^ SafeWyCore.Prime1);
                    break;
                case 5:
                    seed = SafeWyCore.Mum(seed, ((SafeWyCore.Read32(array, p) << 8) | SafeWyCore.Read8(array, p + 4)) ^ SafeWyCore.Prime1);
                    break;
                case 6:
                    seed = SafeWyCore.Mum(seed, ((SafeWyCore.Read32(array, p) << 16) | SafeWyCore.Read16(array, p + 4)) ^ SafeWyCore.Prime1);
                    break;
                case 7:
                    seed = SafeWyCore.Mum(seed, ((SafeWyCore.Read32(array, p) << 24) | (SafeWyCore.Read16(array, p + 4) << 8) | SafeWyCore.Read8(array, p + 6)) ^ SafeWyCore.Prime1);
                    break;
                case 8:
                    seed = SafeWyCore.Mum(seed, SafeWyCore.Read64Swapped(array, p) ^ SafeWyCore.Prime1);
                    break;
                case 9:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read8(array, p + 8) ^ SafeWyCore.Prime2);
                    break;
                case 10:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read16(array, p + 8) ^ SafeWyCore.Prime2);
                    break;
                case 11:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, ((SafeWyCore.Read16(array, p + 8) << 8) | SafeWyCore.Read8(array, p + 10)) ^ SafeWyCore.Prime2);
                    break;
                case 12:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read32(array, p + 8) ^ SafeWyCore.Prime2);
                    break;
                case 13:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, ((SafeWyCore.Read32(array, p + 8) << 8) | SafeWyCore.Read8(array, p + 12)) ^ SafeWyCore.Prime2);
                    break;
                case 14:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, ((SafeWyCore.Read32(array, p + 8) << 16) | SafeWyCore.Read16(array, p + 12)) ^ SafeWyCore.Prime2);
                    break;
                case 15:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, ((SafeWyCore.Read32(array, p + 8) << 24) | (SafeWyCore.Read16(array, p + 12) << 8) | SafeWyCore.Read8(array, p + 14)) ^ SafeWyCore.Prime2);
                    break;
                case 16:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2);
                    break;
                case 17:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(seed, SafeWyCore.Read8(array, p + 16) ^ SafeWyCore.Prime3);
                    break;
                case 18:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(seed, SafeWyCore.Read16(array, p + 16) ^ SafeWyCore.Prime3);
                    break;
                case 19:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(seed, ((SafeWyCore.Read16(array, p + 16) << 8) | SafeWyCore.Read8(array, p + 18)) ^ SafeWyCore.Prime3);
                    break;
                case 20:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(seed, SafeWyCore.Read32(array, p + 16) ^ SafeWyCore.Prime3);
                    break;
                case 21:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(seed, ((SafeWyCore.Read32(array, p + 16) << 8) | SafeWyCore.Read8(array, p + 20)) ^ SafeWyCore.Prime3);
                    break;
                case 22:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(seed, ((SafeWyCore.Read32(array, p + 16) << 16) | SafeWyCore.Read16(array, p + 20)) ^ SafeWyCore.Prime3);
                    break;
                case 23:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(seed, ((SafeWyCore.Read32(array, p + 16) << 24) | (SafeWyCore.Read16(array, p + 20) << 8) | SafeWyCore.Read8(array, p + 22)) ^ SafeWyCore.Prime3);
                    break;
                case 24:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(seed, SafeWyCore.Read64Swapped(array, p + 16) ^ SafeWyCore.Prime3);
                    break;
                case 25:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p + 16) ^ seed, SafeWyCore.Read8(array, p + 24) ^ SafeWyCore.Prime4);
                    break;
                case 26:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p + 16) ^ seed, SafeWyCore.Read16(array, p + 24) ^ SafeWyCore.Prime4);
                    break;
                case 27:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p + 16) ^ seed, ((SafeWyCore.Read16(array, p + 24) << 8) | SafeWyCore.Read8(array, p + 26)) ^ SafeWyCore.Prime4);
                    break;
                case 28:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p + 16) ^ seed, SafeWyCore.Read32(array, p + 24) ^ SafeWyCore.Prime4);
                    break;
                case 29:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p + 16) ^ seed, ((SafeWyCore.Read32(array, p + 24) << 8) | SafeWyCore.Read8(array, p + 28)) ^ SafeWyCore.Prime4);
                    break;
                case 30:
                    seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                           SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p + 16) ^ seed, ((SafeWyCore.Read32(array, p + 24) << 16) | SafeWyCore.Read16(array, p + 28)) ^ SafeWyCore.Prime4);
                    break;
                case 31: seed = SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p) ^ seed, SafeWyCore.Read64Swapped(array, p + 8) ^ SafeWyCore.Prime2) ^ 
                                SafeWyCore.Mum(SafeWyCore.Read64Swapped(array, p + 16) ^ seed, ((SafeWyCore.Read32(array, p + 24) << 24) | (SafeWyCore.Read16(array, p + 28) << 8) | SafeWyCore.Read8(array, p + 30)) ^ SafeWyCore.Prime4);
                    break;
            }

            return seed;
        }
        
        /// <inheritdoc />
        protected override byte[] HashFinal()
        {
            var result = HashFinal(this.seed, this.length);
            return BitConverter.GetBytes(result);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong HashFinal(ulong seed, ulong length) => 
            SafeWyCore.Mum(seed, length ^ SafeWyCore.Prime5);
    }
}
