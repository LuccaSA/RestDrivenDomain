using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class RightExpressionsHelperTests
    {
        [Fact]
        public async Task RightExpressionsHelper()
        {
            var filter = await new OpenRightExpressionsHelper<User>().GetFilterAsync(new Domain.Models.Querying.Query<User> { Verb = Helpers.HttpVerbs.Get });
            Assert.True(filter.Compile()(null));

            filter = await new ClosedRightExpressionsHelper<User>().GetFilterAsync(new Domain.Models.Querying.Query<User> { Verb = Helpers.HttpVerbs.Get });
            Assert.False(filter.Compile()(null));
        }
    }
}