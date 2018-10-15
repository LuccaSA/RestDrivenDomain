using Microsoft.Extensions.Primitives;
using Rdd.Domain.Helpers;
using System.Collections.Generic;
using Rdd.Web.Tests.Models;
using Xunit;

namespace Rdd.Web.Tests
{
    public class OptionsParserTests
    {
        [Fact]
        public void CountParseHasOptionImplications()
        {
            var dico = new Dictionary<string, StringValues> { { "fields", "collection.count" } };
            var query = QueryParserHelper.GetQueryParser<User>().Parse(HttpVerbs.Get, dico, true);

            Assert.True(query.Options.NeedCount);
            Assert.False(query.Options.NeedEnumeration);
        }
    }
}