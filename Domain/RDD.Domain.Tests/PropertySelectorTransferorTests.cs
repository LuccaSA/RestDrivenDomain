using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Tests.Models;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
    public class PropertySelectorTransferorTests
    {
        [Fact]
        public void ExtractionOnSubPropertyShouldWork()
        {
            var ps = ExpressionSelectorChain< DummyClass>.New(d => d.BestChild.DummySubProp);

            Assert.Equal("DummySubProp", ps.Next.ToString());
        }

        [Fact]
        public void ExtractionOnListSubPropertyShouldWork()
        {
            var ps = ExpressionSelectorChain<DummyClass>.New(d => d.Children.Select(c => c.DummySubProp));

            Assert.Equal("DummySubProp", ps.Next.ToString());
        }
    }
}
