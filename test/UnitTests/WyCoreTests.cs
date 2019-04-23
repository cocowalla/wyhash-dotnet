using System;
using System.Numerics;
using Shouldly;
using Xunit;

namespace WyHash.UnitTests
{
    public class WyCoreTests
    {
        [InlineData(2, 2)]
        [InlineData(2_147_483_647, 100)]
        [InlineData(2_147_483_647, 2_147_483_647)]
        [InlineData(2_147_483_647, 100_000_000_000)]
        [InlineData(2_147_483_647, 100_000_000_000_000)]
        [InlineData(UInt64.MaxValue, UInt64.MaxValue)]
        [InlineData(UInt64.MaxValue - 1, UInt64.MaxValue)]
        [InlineData(18446744073709551610, 18446744073709551610)]
        [Theory]
        public void Should_multiply_64_bit_integers(ulong x, ulong y)
        {
            var expectedProduct = BigInteger.Multiply(x, y);
            var expectedHi = (ulong)(expectedProduct >> 64);
            var expectedLo = (ulong)(expectedProduct & 0xfffffffffffffffful);

            // ReSharper disable once UseDeconstruction
            var result = WyCore.Multiply64(x, y);

            result.Hi.ShouldBe(expectedHi);
            result.Lo.ShouldBe(expectedLo);
        }
    }
}
