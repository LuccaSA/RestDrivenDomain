using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
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
            var expression = builder.Like(PropertyExpressionSelector<User>.New(u => u.PictureId), new List<Guid> { goodGuid });

            var goodUser = new User { PictureId = goodGuid };
            var badUser = new User { PictureId = badGuid };

            var compiled = expression.Compile();
            Assert.True(compiled.Invoke(goodUser));
            Assert.False(compiled.Invoke(badUser));
        }
    }
}