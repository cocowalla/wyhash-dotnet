using System.Runtime.InteropServices;
using System.Security;

namespace WyHash.Benchmarks.Native
{
    /// <remarks>
    /// Pinvoke's to a native WyHash version built from the reference C code. Used as a performance comparison against
    /// the managed production code, <seealso cref="WyCore"/>
    /// </remarks>
    public static class NativeWyHash64
    {
        [DllImport("WyHash.Native.dll")]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe ulong WyHash64(byte* input, ulong len, ulong seed);

        public static unsafe ulong ComputeHash64(byte[] array, ulong seed)
        {
            fixed (byte* pData = array)
            {
                return WyHash64(pData, (ulong)array.LongLength, seed);
            }
        }
    }
}
