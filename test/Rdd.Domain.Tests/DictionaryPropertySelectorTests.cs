using Rdd.Domain.Helpers.Expressions;
using System.Collections.Generic;
using Xunit;

namespace Rdd.Domain.Tests
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
            new ExpressionParser().Parse<EntityWithDictionaries>("stringToString.aProperty");
        }

        [Fact]
        public void PropertySelector_ShouldNotFailWhenParsingIntDictionaries()
        {
            new ExpressionParser().Parse<EntityWithDictionaries>("stringToInt.aProperty");
        }

        [Fact]
        public void PropertySelector_ShouldNotFailWhenParsingObjectDictionaries()
        {
            new ExpressionParser().Parse<EntityWithDictionaries>("stringToObject.aProperty");
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