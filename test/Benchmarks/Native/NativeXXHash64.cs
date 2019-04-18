using System.Runtime.InteropServices;
using System.Security;

namespace WyHash.Benchmarks.Native
{
    /// <remarks>
    /// Pinvoke's to a native WyHash version built from the reference C code
    /// </remarks>
    // ReSharper disable once InconsistentNaming
    public static class NativeXXHash64
    {
        [DllImport("xxHash.Native.dll")]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe ulong XXH64(byte* input, ulong len, ulong seed);

        public static unsafe ulong ComputeHash64(byte[] array, ulong seed)
        {
            fixed (byte* pData = array)
            {
                return XXH64(pData, (ulong)array.LongLength, seed);
            }
        }
    }
}
