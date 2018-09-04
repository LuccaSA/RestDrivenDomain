using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using System.Collections.Generic;
using Xunit;

namespace RDD.Domain.Tests
{
    public class PropertySelectorEqualityComparerTests
    {
        private class FakeClass
        {
            public int A { get; set; }
            public int B { get; set; }
        }

        private class FakeClass2
        {
            public int A { get; set; }
            public int B { get; set; }
        }

        [Fact]
        public void ComparingTheSameReferencedPropertyShouldWork()
        {
            var dictionary = new Dictionary<IExpressionSelector, int>(new ExpressionSelectorEqualityComparer());
            var p1 = ExpressionSelectorChain.New((FakeClass fc) => fc.A);
            var p1Val = 42;

            dictionary.Add(p1, p1Val);

            Assert.Equal(p1Val, dictionary[p1]);
        }

        [Fact]
        public void ComparingDifferentPropertyInstancesShouldWork()
        {
            var dictionary = new Dictionary<IExpressionSelector, int>(new ExpressionSelectorEqualityComparer());
            var p1 = ExpressionSelectorChain.New((FakeClass fc) => fc.A);
            var p1Val = 42;
            dictionary.Add(p1, p1Val);

            var p2 = ExpressionSelectorChain.New((FakeClass fc) => fc.A);

            Assert.Equal(p1Val, dictionary[p2]);
        }

        [Fact]
        public void ComparingPropertyInstancesWithDifferentEntityNameButSamePropertyShouldWork()
        {
            var dictionary = new Dictionary<IExpressionSelector, int>(new ExpressionSelectorEqualityComparer());
            var p1 = ExpressionSelectorChain.New((FakeClass fc) => fc.A);
            var p1Val = 42;
            dictionary.Add(p1, p1Val);

            var p2 = ExpressionSelectorChain.New((FakeClass fakeClass) => fakeClass.A);

            Assert.Equal(p1Val, dictionary[p2]);
        }

        [Fact]
        public void ComparingTwoDifferentPropertiesShouldFail()
        {
            var dictionary = new Dictionary<IExpressionSelector, int>(new ExpressionSelectorEqualityComparer());
            var p1 = ExpressionSelectorChain.New((FakeClass fc) => fc.A);
            var p1Val = 42;
            dictionary.Add(p1, p1Val);

            var p2 = ExpressionSelectorChain.New((FakeClass fc) => fc.B);

            Assert.False(dictionary.ContainsKey(p2));
        }

        [Fact]
        public void ComparingPropertiesWithTheSameNameFromTwoDifferentClassesShouldFail()
        {
            var dictionary = new Dictionary<IExpressionSelector, int>(new ExpressionSelectorEqualityComparer());
            var p1 = ExpressionSelectorChain.New((FakeClass fc) => fc.A);
            var p1Val = 42;
            dictionary.Add(p1, p1Val);

            var p2 = ExpressionSelectorChain.New((FakeClass2 fc) => fc.A);

            Assert.False(dictionary.ContainsKey(p2));
        }
    }
}
