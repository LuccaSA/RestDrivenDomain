using RDD.Domain.Helpers;
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
            var collection = new PropertySelector<DummyClass>();

            collection.Add(d => d.DummyProp);

            Assert.True(collection.Contains(d => d.DummyProp));
        }

        [Fact]
        public void SimpleContainsWithDifferentVariableNameShouldWork()
        {
            var collection = new PropertySelector<DummyClass>();

            collection.Add(d => d.DummyProp);

            Assert.True(collection.Contains(c => c.DummyProp));
        }

        [Fact]
        public void SimpleContainsWithDifferentPropsShouldFail()
        {
            var collection = new PropertySelector<DummyClass>();

            collection.Add(d => d.DummyProp);

            Assert.False(collection.Contains(d => d.DummyProp2));
        }

        [Fact]
        public void SubContainsShouldWork()
        {
            var collection = new PropertySelector<DummyClass>();

            collection.Add(d => d.Children.Select(c => c.DummySubSubClass.DummySubSubProp));

            Assert.True(collection.Contains(d => d.Children.Select(c => c.DummySubSubClass.DummySubSubProp)));
        }

        [Fact]
        public void SubAbstractContainsShouldWork()
        {
            var collection = new PropertySelector<DummyClassImpl>();

            collection.Add(d => d.Children.Select(c => c.DummySubSubClass));

            Assert.True(collection.Contains(d => d.Children.Select(c => c.DummySubSubClass)));
        }

        [Fact]
        public void SimpleRemoveShouldWork()
        {
            var collection = new PropertySelector<DummyClass>();

            collection.Add(d => d.DummyProp);

            Assert.True(collection.Remove(d => d.DummyProp));
        }

        [Fact]
        public void RemoveShouldNotRemoveOtherIncludes()
        {
            var collection = new PropertySelector<DummyClass>();

            collection.Add(d => d.BestChild.DummySubProp);
            collection.Add(d => d.BestChild.DummySubProp2);
            collection.Remove(d => d.BestChild.DummySubProp);

            Assert.True(collection.Contains(d => d.BestChild.DummySubProp2), "La suppression de 'BestChild.DummySubProp' ne devrait pas supprimer 'BestChild.DummySubProp2'.");
        }

        [Fact]
        public void RemoveShouldNotRemoveOtherIncludes2Level()
        {
            var collection = new PropertySelector<DummyClass>();

            collection.Add(d => d.BestChild.DummySubSubClass.DummySubSubProp);
            collection.Remove(d => d.BestChild.DummySubSubClass.DummySubSubProp);

            Assert.True(collection.Contains(d => d.BestChild.DummySubSubClass), "La suppression de 'BestChild.DummySubSubClass.DummySubSubProp' ne devrait pas supprimer 'BestChild.DummySubSubClass'.");
        }

        [Fact]
        public void NullRefRemoveShouldWorkWithoutException()
        {
            var collection = new PropertySelector<DummyClass>();

            collection.Add(d => d.DummyProp);

            Assert.False(collection.Remove(d => d.Children.Select(c => c.DummySubProp)));
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
