using System;
using System.Text;
using Shouldly;
using Xunit;

namespace WyHash.UnitTests
{
    public class WyHash64Tests
    {
        // Test vectors from: https://github.com/wangyi-fudan/wyhash
        [InlineData("", 0, "f961f936e29c9345")]
        [InlineData("a", 1, "6dc395f88b363baa")]
        [InlineData("abc", 2, "3bc9d7844798ddaa")]
        [InlineData("message digest", 3, "b31238dc2c500cd3")]
        [InlineData("abcdefghijklmnopqrstuvwxyz", 4, "ea0f542c58cddfe4")]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 5, "1799aca591fe73b4")]
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890", 6, "7f0d02f53d64c1f9")]
        [Theory]
        public void Should_match_test_vectors(string message, ulong seed, string expected)
        {
            var data = Encoding.UTF8.GetBytes(message);
            
            var result = WyHash64.ComputeHash64(data, seed);
            var actual = $"{result:x}";
            
            actual.ShouldBe(expected);
        }
        
        // Test that covers all remainders in WyHashCore
        [InlineData(1, "67a93d2d9b2a642c")]
        [InlineData(2, "5a47d358d4db3848")]
        [InlineData(3, "643513fa1dbd477c")]
        [InlineData(4, "9958134fac91cf39")]
        [InlineData(5, "606e0649e1d78d6")]
        [InlineData(6, "98471b5e87eae8e7")]
        [InlineData(7, "7dc769b9439ff8b3")]
        [InlineData(8, "4bf9ac0240608c5f")]
        [InlineData(9, "36932d8a49021276")]
        [InlineData(10, "1bcef7b64fe6abfc")]
        [InlineData(11, "d424052d8aa20d8a")]
        [InlineData(12, "ba536415e9835525")]
        [InlineData(13, "d8d0fc4425b406c4")]
        [InlineData(14, "96cb19957d99b2b3")]
        [InlineData(15, "777400af0b535b91")]
        [InlineData(16, "3a308981a6e56174")]
        [InlineData(17, "e99ef85145239f65")]
        [InlineData(18, "c7f6a457392e6b")]
        [InlineData(19, "759964be1ccff12b")]
        [InlineData(20, "b80e1393662e82be")]
        [InlineData(21, "7cd1f87e6752476")]
        [InlineData(22, "2e4e513b55ac1da0")]
        [InlineData(23, "721df7ecac59bbeb")]
        [InlineData(24, "ccaf9abf790d1aec")]
        [InlineData(25, "f65b2d70823c2730")]
        [InlineData(26, "7edd64adbbcb64c0")]
        [InlineData(27, "1e19275b5b5861a")]
        [InlineData(28, "6d5747b7007c27dd")]
        [InlineData(29, "e6d1e3460b176e3c")]
        [InlineData(30, "620d877a436f39bf")]
        [InlineData(31, "a38f6e93134feeca")]
        [InlineData(32, "94e1321636f65c3e")] // No remainder
        [Theory]
        public void Should_hash_data(int count, string expected)
        {
            var message = new String('a', count);
            var data = Encoding.UTF8.GetBytes(message);
            
            var result = WyHash64.ComputeHash64(data, 42);
            var actual = $"{result:x}";
            
            actual.ShouldBe(expected);
        }
    
        [Fact]
        public void Should_return_8_bytes()
        {
            var data = new byte[] { 0, 1, 2, };
            var hasher = WyHash64.Create(42);

            var result = hasher.ComputeHash(data);
            result.Length.ShouldBe(8);
            
            // 64 bits/8 bytes
            hasher.HashSize.ShouldBe(64);
        }
    }
}