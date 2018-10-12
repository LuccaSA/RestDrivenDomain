using Moq;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Models;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using System.Collections.Generic;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class RightExpressionsHelperTests
    {
        [Fact]
        public void RightExpressionsHelper()
        {
            var holder = new Mock<ICombinationsHolder>();
            holder.Setup(h => h.Combinations).Returns(new List<Combination>());

            var anonymousHelper = new RightExpressionsHelper<User>(null, holder.Object);
            Assert.Throws<ForbiddenException>(() => anonymousHelper.GetFilter(null));

            var helper = new RightExpressionsHelper<User>(new Mock<IPrincipal>().Object, holder.Object);
            Assert.Throws<UnreachableEntityException>(() => helper.GetFilter(new Domain.Models.Querying.Query<User>()));


            holder.Setup(h => h.Combinations).Returns(new List<Combination> { new Combination { Verb = Helpers.HttpVerbs.Get, Subject = typeof(User), Operation = new Operation() } });
            var filter = helper.GetFilter(new Domain.Models.Querying.Query<User> { Verb = Helpers.HttpVerbs.Get });

            Assert.True(filter.Compile()(null));
        }
    }
}