﻿using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
    public class DictionaryPropertySelectorTests
    {
        private class EntityWithDictionaries
        {
            public Dictionary<string, string> StringToString { get; set; }
            public Dictionary<string, int> StringToInt { get; set; }
            public Dictionary<string, object> StringToObject { get; set; }
        }

        [Fact]
        public void PropertySelector_ShouldNotFailWhenParsingStringDictionaries()
        {
            var ps = new ExpressionParser().Parse<EntityWithDictionaries>("stringToString.aProperty");
        }

        [Fact]
        public void PropertySelector_ShouldNotFailWhenParsingIntDictionaries()
        {
            var ps = new ExpressionParser().Parse<EntityWithDictionaries>("stringToInt.aProperty");
        }

        [Fact]
        public void PropertySelector_ShouldNotFailWhenParsingObjectDictionaries()
        {
            var ps = new ExpressionParser().Parse<EntityWithDictionaries>("stringToObject.aProperty");
        }

        [Fact]
        public void DictionaryPropertySelector_ShouldGenerateLambdaDicoAccessor_OnQueriedProperty()
        {
            var ps = new ExpressionParser().ParseChain<EntityWithDictionaries>("stringToString.aProperty");

            Assert.Equal("StringToString", ps.Current.ToString());
            Assert.Equal("aProperty", ps.Next.ToString());
            Assert.Equal("StringToString.aProperty", ps.ToString());
        }
    }
}