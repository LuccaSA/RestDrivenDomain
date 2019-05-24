using Rdd.Domain.Helpers;
using Rdd.Web.Tests.Models;
using Xunit;

namespace Rdd.Web.Tests
{
    public class OptionsParserTests
    {
        [Fact]
        public void CountParseHasOptionImplications()
        {
            var request = HttpVerbs.Get.NewRequest(("fields", "collection.count"));

            var query = QueryParserHelper.GetQueryParser<User>().Parse(request, true);

            Assert.True(query.Options.NeedsCount);
            Assert.False(query.Options.NeedsEnumeration);
        }
    }
}