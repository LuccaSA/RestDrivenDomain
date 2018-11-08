using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Infra.Helpers;
using Rdd.Web.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Rdd.Domain.Models.Querying;
using Xunit;

namespace Rdd.Web.Tests
{
    public class QueryBuilderTests
    {
        [Fact]
        public void LikeOnGuidsShouldWork()
        {
            var goodGuid = Guid.NewGuid();
            var badGuid = Guid.NewGuid();
            var builder = new WebFilterConverter<User>();
            var expression = builder.Like(PropertyExpression<User>.New(u => u.PictureId), goodGuid.AsFilterValue());

            var goodUser = new User { PictureId = goodGuid };
            var badUser = new User { PictureId = badGuid };

            var compiled = expression.Compile();
            Assert.True(compiled.Invoke(goodUser));
            Assert.False(compiled.Invoke(badUser));
        }

        [Fact]
        public void Anniversary()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.Anniversary(PropertyExpression<User>.New(u => u.BirthDay), ((DateTime?)DateTime.Today).AsFilterValue());

            var goodUser = new User { BirthDay = DateTime.Today };
            var badUser = new User { BirthDay = DateTime.Today.AddDays(1) };
            var nullUser = new User { };

            var users = new List<User> { goodUser, badUser, nullUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void AnniversaryNotNullable()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.Anniversary(PropertyExpression<User>.New(u => u.ContractStart), DateTime.Today.AsFilterValue());

            var goodUser = new User { ContractStart = DateTime.Today };
            var badUser = new User { ContractStart = DateTime.Today.AddDays(1) };

            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void NullAnniversary()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.Anniversary(PropertyExpression<User>.New(u => u.BirthDay), new DateTime?[] { DateTime.Today, null }.AsFilterValueArray());

            var goodUser = new User { BirthDay = DateTime.Today };
            var badUser = new User { BirthDay = DateTime.Today.AddDays(1) };
            var nullUser = new User { };

            var users = new List<User> { goodUser, badUser, nullUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], nullUser);
        }

        [Fact]
        public void UntilOnDatetime()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.Anniversary(PropertyExpression<User>.New(u => u.BirthDay), new DateTime?[] { DateTime.Today, null }.AsFilterValueArray());

            var goodUser = new User { BirthDay = DateTime.Today };
            var badUser = new User { BirthDay = DateTime.Today.AddDays(1) };
            var nullUser = new User { };

            var users = new List<User> { goodUser, badUser, nullUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], nullUser);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1, 2)]
        public void EqualsFilter(params int[] values)
        {
            var builder = new WebFilterConverter<User>();
            IFilterValue filterValue = values.Length == 1 ? (IFilterValue)values[0].AsFilterValue() : values.AsFilterValueArray();
            var expression = builder.Equal(PropertyExpression<User>.New(u => u.Id), filterValue);

            var goodUser = new User { Id = 1 };
            var badUser = new User { Id = 3 };

            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(1, 2)]
        public void NotEqualsFilter(params int[] values)
        {
            var builder = new WebFilterConverter<User>();
            IFilterValue filterValue = values.Length == 1 ? (IFilterValue)values[0].AsFilterValue() : values.AsFilterValueArray();
            var expression = builder.NotEqual(PropertyExpression<User>.New(u => u.Id), filterValue);

            var goodUser = new User { Id = 3 };
            var badUser = new User { Id = 2 };

            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void ContainsAllFilter()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.ContainsAll(PropertyExpression<User>.New(u => u.Id), new[] { 1, 2 }.AsFilterValueArray());

            var badUser = new User { Id = 3 };
            var badUser2 = new User { Id = 2 };

            var users = new List<User> { badUser2, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Empty(users);
        }

        [Fact]
        public void GreaterThanOrEqualFilter()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.GreaterThanOrEqual(PropertyExpression<User>.New(u => u.Id), new[] { 1, 2 }.AsFilterValueArray());

            var goodUser = new User { Id = 3 };
            var goodUser2 = new User { Id = 1 };
            var badUser = new User { Id = 0 };

            var users = new List<User> { goodUser, goodUser2, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], goodUser2);
        }

        [Fact]
        public void LessThanOrEqualFilter()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.LessThanOrEqual(PropertyExpression<User>.New(u => u.Id), new[] { 1, 2 }.AsFilterValueArray());

            var goodUser = new User { Id = 0 };
            var goodUser2 = new User { Id = 1 };
            var badUser = new User { Id = 3 };

            var users = new List<User> { goodUser, goodUser2, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], goodUser2);
        }

        [Fact]
        public void GreaterThanFilter()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.GreaterThan(PropertyExpression<User>.New(u => u.Id), new[] { 1, 2 }.AsFilterValueArray());

            var goodUser = new User { Id = 3 };
            var badUser = new User { Id = 1 };

            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void LessThanFilter()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.LessThan(PropertyExpression<User>.New(u => u.Id), new[] { 1, 2 }.AsFilterValueArray());

            var goodUser = new User { Id = 1 };
            var badUser = new User { Id = 2 };

            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void BetweenFilter()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.Between(PropertyExpression<User>.New(u => u.ContractStart), new Period(DateTime.Today, DateTime.Today.AddDays(1)).AsFilterValue());

            var goodUser = new User { ContractStart = DateTime.Today.AddHours(12) };
            var badUser = new User { ContractStart = DateTime.Today.AddDays(2) };

            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void StartsFilter()
        {
            var builder = new WebFilterConverter<User>();
            var expression = builder.Starts(PropertyExpression<User>.New(u => u.Name), new[] { "aAa", "bbb" }.AsFilterValueArray());

            var goodUser = new User { Name = "AAAseed" };
            var goodUser2 = new User { Name = "bBbseed" };
            var badUser = new User { Name = "edgseed" };

            var users = new List<User> { goodUser, goodUser2, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], goodUser2);
        }
    }
}