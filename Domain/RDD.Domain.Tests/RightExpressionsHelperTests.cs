using Moq;
using RDD.Domain.Exceptions;
using RDD.Domain.Models;
using RDD.Domain.Rights;
using RDD.Domain.Tests.Models;
using System.Collections.Generic;
using Xunit;

namespace RDD.Domain.Tests
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


            holder.Setup(h => h.Combinations).Returns(new List<Combination> { new Combination { Verb = Helpers.HttpVerb.Get, Subject = typeof(User), Operation = new Operation() } });
            var filter = helper.GetFilter(new Domain.Models.Querying.Query<User> { Verb = Helpers.HttpVerb.Get });

            Assert.True(filter.Compile()(null));
        }
    }
}