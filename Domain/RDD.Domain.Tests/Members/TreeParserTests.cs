using Rdd.Domain.Helpers.Expressions.Utils;
using System;
using Xunit;

namespace Rdd.Domain.Tests.Members
{
    public class TreeParserTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(" ,   , ,", null)]
        [InlineData("aaa", "aaa")]
        [InlineData(",aaa", "aaa")]
        [InlineData("aaa,", "aaa")]
        [InlineData("a,b,c,", "[a,b,c]")]
        [InlineData("aaa,bbb", "[aaa,bbb]")]
        [InlineData("a[b,c,d,]", "a[b,c,d]")]
        [InlineData("aaa.", "aaa")]
        [InlineData("aaa.,bbb", "[aaa,bbb]")]
        [InlineData("aaa, bbb", "[aaa,bbb]")]
        [InlineData("a.a.a,b.b.b", "[a.a.a,b.b.b]")]
        [InlineData("a[a.a],b.b[b]", "[a.a.a,b.b.b]")]
        [InlineData("a[a,b,b.b[b]]", "a[a,b.b.b]")]
        [InlineData("a.b.c,a.b.d", "a.b[c,d]")]
        [InlineData("a.b.c, a.d.e", "a[b.c,d.e]")]
        [InlineData("a.b[c, d, ] , a.b, a.b.e", "a.b[c,d,e]")]
        public void NormalUseCases(string input, string output)
        {
            var parser = new TreeParser();
            var tree = parser.Parse(input);

            Assert.Equal(output, tree.ToString());
        }

        [Theory]
        [InlineData(".")]
        [InlineData("[")]
        [InlineData("]][[")]
        [InlineData("[[]")]
        [InlineData(".aaa")]
        [InlineData("a  aa")]
        [InlineData("a.[b]")]
        public void FailedCases(string input)
        {
            var parser = new TreeParser();

            Assert.Throws<FormatException>(()=> parser.Parse(input));
        }
    }
}