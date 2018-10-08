using Microsoft.Extensions.Primitives;
using Rdd.Domain.Helpers;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Web.Querying;
using Rdd.Web.Tests.Models;
using System.Collections.Generic;
using Xunit;

namespace Rdd.Web.Tests
{
    public class OptionsParserTests
    {
        [Fact]
        public void CountParseHasOptionImplications()
        {
            var dico = new Dictionary<string, StringValues> { { "fields", "collection.count" } };
            var parser = new QueryParser<User>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<User>());
            var query = parser.Parse(HttpVerbs.Get, dico, true);

            Assert.True(query.Options.NeedCount);
            Assert.False(query.Options.NeedEnumeration);
        }
    }
}