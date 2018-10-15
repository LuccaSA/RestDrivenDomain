using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class RightExpressionsHelperTests
    {
        [Fact]
        public void RightExpressionsHelper()
        {
            var filter = new OpenRightExpressionsHelper<User>().GetFilter(new Domain.Models.Querying.Query<User> { Verb = Helpers.HttpVerbs.Get });
            Assert.True(filter.Compile()(null));

            filter = new ClosedRightExpressionsHelper<User>().GetFilter(new Domain.Models.Querying.Query<User> { Verb = Helpers.HttpVerbs.Get });
            Assert.False(filter.Compile()(null));
        }
    }
}