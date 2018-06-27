using RDD.Domain.Helpers;
using RDD.Domain.Tests.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace RDD.Domain.Tests
{
    public class PropertySelectorTransferorTests
    {
        [Fact]
        public void ExtractionOnSubPropertyShouldWork()
        {
            Expression<Func<DummyClass, object>> expression = d => d.BestChild.DummySubProp;

            var extractor = new PropertySelectorTransferor<DummyClass, DummySubClass>("BestChild");
            var result = extractor.Visit(expression);

            Assert.Equal("p => p.DummySubProp", result.ToString());
        }

        [Fact]
        public void ExtractionOnListSubPropertyShouldWork()
        {
            Expression<Func<DummyClass, object>> expression = d => d.Children.Select(c => c.DummySubProp);

            var extractor = new PropertySelectorTransferor<DummyClass, DummySubClass>("Children");
            var result = extractor.Visit(expression);

            Assert.Equal("c => c.DummySubProp", result.ToString());
        }
    }
}
