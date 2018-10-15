using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Tests.Models;
using System.Linq;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class PropertySelectorTransferorTests
    {
        [Fact]
        public void ExtractionOnSubPropertyShouldWork()
        {
            var ps = ExpressionChain< DummyClass>.New(d => d.BestChild.DummySubProp);

            Assert.Equal("DummySubProp", ps.Next.ToString());
        }

        [Fact]
        public void ExtractionOnListSubPropertyShouldWork()
        {
            var ps = ExpressionChain<DummyClass>.New(d => d.Children.Select(c => c.DummySubProp));

            Assert.Equal("DummySubProp", ps.Next.ToString());
        }
    }
}
