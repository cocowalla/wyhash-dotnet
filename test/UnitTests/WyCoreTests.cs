using Shouldly;
using Xunit;

namespace WyHash.UnitTests
{
    public class WyCoreTests
    {
        [InlineData(2, 2, 0, 4)]
        [InlineData(2_147_483_647, 100, 0, 214_748_364_700)]
        [InlineData(2_147_483_647, 2_147_483_647, 0, 4_611_686_014_132_420_609)]
        [InlineData(2_147_483_647, 100_000_000_000, 11, 11_834_179_889_194_932_224)]
        [InlineData(2_147_483_647, 100_000_000_000_000, 11_641, 9_816_937_947_109_638_144)]
        [Theory]
        public void Should_multiply_64_bit_integers(ulong x, ulong y, ulong expectedHi, ulong expectedLo)
        {
            // ReSharper disable once UseDeconstruction
            var result = WyCore.Multiply64(x, y);
            
            result.Hi.ShouldBe(expectedHi);
            result.Lo.ShouldBe(expectedLo);
        }
    }
}
