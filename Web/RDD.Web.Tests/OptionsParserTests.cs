using Microsoft.Extensions.Primitives;
using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Infra.Helpers;
using RDD.Web.Querying;
using RDD.Web.Tests.Models;
using System.Collections.Generic;
using Xunit;

namespace RDD.Web.Tests
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