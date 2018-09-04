using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Tests.Models;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RDD.Domain.Tests
{
    public class PropertySelectorCollectionTests
    {
        [Fact]
        public void SimpleContainsShouldWork()
        {
            var collection = ExpressionSelectorChain.New((DummyClass d) => d.DummyProp);

            Assert.True(collection.Contains((DummyClass d) => d.DummyProp));
        }

        [Fact]
        public void SimpleContainsWithDifferentVariableNameShouldWork()
        {
            var collection = ExpressionSelectorChain.New((DummyClass d) => d.DummyProp);

            Assert.True(collection.Contains((DummyClass c) => c.DummyProp));
        }

        [Fact]
        public void SimpleContainsWithDifferentPropsShouldFail()
        {
            var collection = ExpressionSelectorChain.New((DummyClass d) => d.DummyProp);

            Assert.False(collection.Contains((DummyClass d) => d.DummyProp2));
        }

        [Fact]
        public void SubContainsShouldWork()
        {
            var collection = ExpressionSelectorChain.New((DummyClass d) => d.Children.Select(c => c.DummySubSubClass.DummySubSubProp));

            Assert.True(collection.Contains((DummyClass d) => d.Children.Select(c => c.DummySubSubClass.DummySubSubProp)));
        }

        [Fact]
        public void SubAbstractContainsShouldWork()
        {
            var collection = ExpressionSelectorChain.New((DummyClass d) => d.Children.Select(c => c.DummySubSubClass));

            Assert.True(collection.Contains((DummyClass d) => d.Children.Select(c => c.DummySubSubClass)));
        }

        [Fact]
        public void RegexReplace01ShouldWork()
        {
            var exp = "p => Convert(p.Id)";

            var result = Regex.Replace(exp, "Convert\\((.*)\\)", "$1");

            Assert.Equal("p => p.Id", result);
        }

        [Fact]
        public void RegexReplace02houldWork()
        {
            var exp = "p => p.Users.Select(pp => Convert(pp.Id))";

            var result = Regex.Replace(exp, "Convert\\((.*)\\)", "$1");

            Assert.Equal("p => p.Users.Select(pp => pp.Id)", result);
        }

        [Fact]
        public void RegexReplace03houldWork()
        {
            var exp = "p => p.Select(pp => Convert(pp.Users.Select(ppp => ppp.LegalEntity.Country)))";

            var result = Regex.Replace(exp, "Convert\\((.*)\\)", "$1");

            Assert.Equal("p => p.Select(pp => pp.Users.Select(ppp => ppp.LegalEntity.Country))", result);
        }
    }
}
