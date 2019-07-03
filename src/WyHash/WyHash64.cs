using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes
namespace WyHash
{
    /// <inheritdoc />
    /// <summary>
    /// .NET implementation of the wyhash 64-bit hash algorithm by Wang Yi. Reference implementation (version 20190328):
    /// https://github.com/wangyi-fudan/wyhash/blob/master/wyhash.h
    /// </summary>
    public class WyHash64 : HashAlgorithm
    {
        private ulong seed;
        private ulong length;
        
        /// <inheritdoc cref="HashAlgorithm.HashSize"/>    
        public override int HashSize => 64;

        public new static WyHash64 Create() => new WyHash64();
        public static WyHash64 Create(ulong seed) => new WyHash64(seed);

        private WyHash64() { }
        
        private WyHash64(ulong seed = 0)
        {
            this.seed = seed;
        }
        
        /// <summary>
        /// Convenience method to compute a WyHash hash and return the result as a 64-bit unsigned integer
        /// </summary>
        public static ulong ComputeHash64(byte[] array, ulong seed = 0)
        {
            seed = WyHashCore(array.AsSpan(), seed);
            return HashFinal(seed, (ulong)array.Length);
        }

        /// <summary>
        /// Convenience method to compute a WyHash hash and return the result as a 64-bit unsigned integer
        /// </summary>
        public static ulong ComputeHash64(ReadOnlySpan<byte> data, ulong seed = 0)
        {
            seed = WyHashCore(data, seed);
            return HashFinal(seed, (ulong)data.Length);
        }

        /// <summary>
        /// Convenience method to compute a WyHash hash and return the result as a 64-bit unsigned integer
        /// </summary>
        public static ulong ComputeHash64(ReadOnlySpan<char> str, ulong seed = 0)
        {
            var data = MemoryMarshal.Cast<char, byte>(str);
            seed = WyHashCore(data, seed);
            return HashFinal(seed, (ulong)data.Length);
        }

        /// <summary>
        /// Convenience method to compute a WyHash hash and return the result as a 64-bit unsigned integer
        /// </summary>
        public static ulong ComputeHash64(string str, ulong seed = 0)
        {
            var data = MemoryMarshal.Cast<char, byte>(str.AsSpan());
            seed = WyHashCore(data, seed);
            return HashFinal(seed, (ulong)data.Length);
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

            this.seed = WyHashCore(array.AsSpan(ibStart, cbSize), this.seed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong WyHashCore(ReadOnlySpan<byte> span, ulong seed)
        {
            fixed (byte* pData = span)
            {
                byte* ptr = pData;
                
                var len = span.Length;
                var p = 0;
                
                for (int i = 0; i + 32 <= len; i += 32, p += 32)
                {
                    // Storing these in temp variables is slightly more performant (presumably it gives some kind of hint to the jitter)
                    var m1x = WyCore.Read64(ptr, p) ^ WyCore.Prime1;
                    var m1y = WyCore.Read64(ptr, p + 8) ^ WyCore.Prime2;
                    var m2x = WyCore.Read64(ptr, p + 16) ^ WyCore.Prime3;
                    var m2y = WyCore.Read64(ptr, p + 24) ^ WyCore.Prime4;
                        
                    seed = WyCore.Mum(seed ^ WyCore.Prime0, WyCore.Mum(m1x, m1y) ^ WyCore.Mum(m2x, m2y));
                }

                seed ^= WyCore.Prime0;

                // After the loop we have between 1 and 31 bytes left to process
                switch (len & 31)
                {
                    case 1:
                        seed = WyCore.Mum(seed, WyCore.Read8(ptr, p) ^ WyCore.Prime1);
                        break;
                    case 2:
                        seed = WyCore.Mum(seed, WyCore.Read16(ptr, p) ^ WyCore.Prime1);
                        break;
                    case 3:
                        seed = WyCore.Mum(seed, ((WyCore.Read16(ptr, p) << 8) | WyCore.Read8(ptr, p + 2)) ^ WyCore.Prime1);
                        break;
                    case 4:
                        seed = WyCore.Mum(seed, WyCore.Read32(ptr, p) ^ WyCore.Prime1);
                        break;
                    case 5:
                        seed = WyCore.Mum(seed, ((WyCore.Read32(ptr, p) << 8) | WyCore.Read8(ptr, p + 4)) ^ WyCore.Prime1);
                        break;
                    case 6:
                        seed = WyCore.Mum(seed, ((WyCore.Read32(ptr, p) << 16) | WyCore.Read16(ptr, p + 4)) ^ WyCore.Prime1);
                        break;
                    case 7:
                        seed = WyCore.Mum(seed, ((WyCore.Read32(ptr, p) << 24) | (WyCore.Read16(ptr, p + 4) << 8) | WyCore.Read8(ptr, p + 6)) ^ WyCore.Prime1);
                        break;
                    case 8:
                        seed = WyCore.Mum(seed, WyCore.Read64Swapped(ptr, p) ^ WyCore.Prime1);
                        break;
                    case 9:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read8(ptr, p + 8) ^ WyCore.Prime2);
                        break;
                    case 10:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read16(ptr, p + 8) ^ WyCore.Prime2);
                        break;
                    case 11:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, ((WyCore.Read16(ptr, p + 8) << 8) | WyCore.Read8(ptr, p + 10)) ^ WyCore.Prime2);
                        break;
                    case 12:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read32(ptr, p + 8) ^ WyCore.Prime2);
                        break;
                    case 13:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, ((WyCore.Read32(ptr, p + 8) << 8) | WyCore.Read8(ptr, p + 12)) ^ WyCore.Prime2);
                        break;
                    case 14:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, ((WyCore.Read32(ptr, p + 8) << 16) | WyCore.Read16(ptr, p + 12)) ^ WyCore.Prime2);
                        break;
                    case 15:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, ((WyCore.Read32(ptr, p + 8) << 24) | (WyCore.Read16(ptr, p + 12) << 8) | WyCore.Read8(ptr, p + 14)) ^ WyCore.Prime2);
                        break;
                    case 16:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2);
                        break;
                    case 17:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(seed, WyCore.Read8(ptr, p + 16) ^ WyCore.Prime3);
                        break;
                    case 18:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(seed, WyCore.Read16(ptr, p + 16) ^ WyCore.Prime3);
                        break;
                    case 19:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(seed, ((WyCore.Read16(ptr, p + 16) << 8) | WyCore.Read8(ptr, p + 18)) ^ WyCore.Prime3);
                        break;
                    case 20:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(seed, WyCore.Read32(ptr, p + 16) ^ WyCore.Prime3);
                        break;
                    case 21:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(seed, ((WyCore.Read32(ptr, p + 16) << 8) | WyCore.Read8(ptr, p + 20)) ^ WyCore.Prime3);
                        break;
                    case 22:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(seed, ((WyCore.Read32(ptr, p + 16) << 16) | WyCore.Read16(ptr, p + 20)) ^ WyCore.Prime3);
                        break;
                    case 23:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(seed, ((WyCore.Read32(ptr, p + 16) << 24) | (WyCore.Read16(ptr, p + 20) << 8) | WyCore.Read8(ptr, p + 22)) ^ WyCore.Prime3);
                        break;
                    case 24:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(seed, WyCore.Read64Swapped(ptr, p + 16) ^ WyCore.Prime3);
                        break;
                    case 25:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(WyCore.Read64Swapped(ptr, p + 16) ^ seed, WyCore.Read8(ptr, p + 24) ^ WyCore.Prime4);
                        break;
                    case 26:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(WyCore.Read64Swapped(ptr, p + 16) ^ seed, WyCore.Read16(ptr, p + 24) ^ WyCore.Prime4);
                        break;
                    case 27:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(WyCore.Read64Swapped(ptr, p + 16) ^ seed, ((WyCore.Read16(ptr, p + 24) << 8) | WyCore.Read8(ptr, p + 26)) ^ WyCore.Prime4);
                        break;
                    case 28:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(WyCore.Read64Swapped(ptr, p + 16) ^ seed, WyCore.Read32(ptr, p + 24) ^ WyCore.Prime4);
                        break;
                    case 29:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(WyCore.Read64Swapped(ptr, p + 16) ^ seed, ((WyCore.Read32(ptr, p + 24) << 8) | WyCore.Read8(ptr, p + 28)) ^ WyCore.Prime4);
                        break;
                    case 30:
                        seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                               WyCore.Mum(WyCore.Read64Swapped(ptr, p + 16) ^ seed, ((WyCore.Read32(ptr, p + 24) << 16) | WyCore.Read16(ptr, p + 28)) ^ WyCore.Prime4);
                        break;
                    case 31: seed = WyCore.Mum(WyCore.Read64Swapped(ptr, p) ^ seed, WyCore.Read64Swapped(ptr, p + 8) ^ WyCore.Prime2) ^ 
                                    WyCore.Mum(WyCore.Read64Swapped(ptr, p + 16) ^ seed, ((WyCore.Read32(ptr, p + 24) << 24) | (WyCore.Read16(ptr, p + 28) << 8) | WyCore.Read8(ptr, p + 30)) ^ WyCore.Prime4);
                        break;
                }
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
            WyCore.Mum(seed, length ^ WyCore.Prime5);
    }
}
