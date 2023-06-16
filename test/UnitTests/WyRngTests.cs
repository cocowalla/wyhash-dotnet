using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace WyHash.UnitTests;

public class WyRngTests
{
    // Sequence of values generated for the original C reference code, for seed 42
    private static readonly string[] Expected =
    {
        "ae4a7cbfdda9b434",
        "e9cc09d33d38d9d2",
        "cb5756512b93433a",
        "eb29b2a1320e1a71",
        "5a3bd6480ed396c0",
        "ec3e2f1427e4b84d",
        "3a990922669eaad2",
        "161299c188b6857d",
        "e18bb8c4a5ad5e5f",
        "7685f82174adb46b"
    };

    [Fact]
    public void Should_match_original_values()
    {
        var rng = new WyRng(42);

        for (int i = 0; i < 10; ++i)
        {
            var result = rng.NextLong();
            $"{result:x}".ShouldBe(Expected[i]);
        }
    }

    /// <summary>
    /// When we get a random int, we're really just generating a long and casting it to int, so
    /// test we generate a long for each call to Next()
    /// </summary>
    [Fact]
    public void Should_generate_long_for_int()
    {
        var rng = new WyRng(42);

        for (int i = 0; i < 9; ++i)
        {
            rng.Next();
        }

        // We should have generated 9 longs in the loop, so we know what to expect for the next long we generate
        var result = rng.NextLong();
        $"{result:x}".ShouldBe(Expected[9]);
    }

    [Fact]
    public void Should_generate_long_for_8_bytes()
    {
        var rng = new WyRng(42);

        // We should only need to generate 1 long to fill 8 bytes...
        var buffer = new byte[8];
        rng.NextBytes(buffer);
        BitConverter.ToString(buffer.Reverse().ToArray()).Replace("-", "").ToLower().ShouldBe(Expected[0]);

        // ...so we know what to expect for the next long we generate
        var result = rng.NextLong();
        $"{result:x}".ShouldBe(Expected[1]);
    }
}
