using RDD.Domain.Helpers;
using RDD.Infra.Helpers;
using RDD.Web.Tests.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace RDD.Web.Tests
{
    public class QueryBuilderTests
    {
        [Fact]
        public void LikeOnGuidsShouldWork()
        {
            var goodGuid = Guid.NewGuid();
            var badGuid = Guid.NewGuid();
            var builder = new QueryBuilder<User, int>();
            var expression = builder
                .Like(new PropertySelector<User>(u => u.PictureId), new List<Guid> { goodGuid })
                .Compile();

            var goodUser = new User { PictureId = goodGuid };
            var badUser = new User { PictureId = badGuid };

            Assert.True(expression.Invoke(goodUser));
            Assert.False(expression.Invoke(badUser));
        }
    }
}
