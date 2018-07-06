using RDD.Domain.Helpers;
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
            var dico = new EntityWithDictionaries();
            var ps = new PropertySelector<EntityWithDictionaries>();

            ps.Parse("stringToString.aProperty");
        }

        [Fact]
        public void PropertySelector_ShouldNotFailWhenParsingIntDictionaries()
        {
            var dico = new EntityWithDictionaries();
            var ps = new PropertySelector<EntityWithDictionaries>();

            ps.Parse("stringToInt.aProperty");
        }

        [Fact]
        public void PropertySelector_ShouldNotFailWhenParsingObjectDictionaries()
        {
            var dico = new EntityWithDictionaries();
            var ps = new PropertySelector<EntityWithDictionaries>();

            ps.Parse("stringToObject.aProperty");
        }

        [Fact]
        public void DictionaryPropertySelector_ShouldGenerateLambdaDicoAccessor_OnQueriedProperty()
        {
            var dico = new EntityWithDictionaries();
            var ps = new PropertySelector<EntityWithDictionaries>();

            ps.Parse("stringToString.aProperty");

            Assert.Equal("p => p.StringToString", ps.Lambda.ToString());
            Assert.Equal(@"pp => pp.Item[""aProperty""]", ps.Child.Lambda.ToString());
        }
    }
}
