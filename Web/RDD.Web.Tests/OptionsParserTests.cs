using RDD.Web.Tests.Models;
using Xunit;
using RDD.Web.Querying;
using System.Collections.Generic;

namespace RDD.Web.Tests
{
    public class OptionsParserTests
    {
        [Fact]
        public void CountParseHasOptionImplications()
        {
            var dico = new Dictionary<string, string> { { "fields", "collection.count" } };
            var fields = new FieldsParser().ParseFields<User>(dico, true);
            var options = new OptionsParser().Parse(dico, fields);

            Assert.True(options.NeedCount);
            Assert.False(options.NeedEnumeration);
        }
    }
}