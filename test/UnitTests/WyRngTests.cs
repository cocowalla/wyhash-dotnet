using System;
using Shouldly;
using Xunit;

namespace WyHash.UnitTests
{
    public class WyRngTests
    {
        // Tests that we generate the same values as the original c reference code
        [Fact]
        public void Should_match_original_values()
        {
            var rng = new WyRng(42);
            
            var r1 = rng.NextLong();
            var r2 = rng.Next();
            var r3 = rng.NextLong();

            var buffer = new byte[12];
            rng.NextBytes(buffer);
            var r4 = BitConverter.ToString(buffer).Replace("-", "").ToLower();
            
            var r5 = rng.NextLong();
            
            r1.ShouldBe(12558987674375533620);
            r2.ShouldBe(805624350);
            r3.ShouldBe(17418380496519466978);
            r4.ShouldBe("a23954acf293409ddcb3958c");
            r5.ShouldBe(6581057395178234814ul);
        }
    }
}
