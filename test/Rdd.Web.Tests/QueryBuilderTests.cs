using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Infra.Exceptions;
using Rdd.Infra.Helpers;
using Rdd.Infra.Web.Models;
using Rdd.Web.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var goodUser = new User { PictureId = goodGuid };
            var badUser = new User { PictureId = badGuid };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.PictureId), WebFilterOperand.Like, new List<Guid> { goodGuid });
            var expression = builder.ToExpression(filter);
            var compiled = expression.Compile();
            Assert.True(compiled.Invoke(goodUser));
            Assert.False(compiled.Invoke(badUser));

            Assert.Throws<QueryBuilderException>(() => builder.Like(filter.Expression, Enumerable.Range(0, 100000).ToList()));
        }

        [Theory]
        [InlineData("abc", true)]
        [InlineData("aBc", true)]
        [InlineData("AbCdE", true)]
        [InlineData("eeeeeabc", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("ab", false)]
        [InlineData("aedzbffc", false)]
        public void LikeOnNullableString(string name, bool result)
        {
            var pattern = "abc";
            var builder = new WebFilterConverter<User>();
            var user = new User { Name = name };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.Name), WebFilterOperand.Like, new List<string> { pattern });
            var expression = builder.ToExpression(filter);
            var compiled = expression.Compile();
            Assert.Equal(result, compiled.Invoke(user));
        }

        [Theory]
        [InlineData("2019-01-01", true)]
        [InlineData(null, false)]
        [InlineData("2029-01-01", false)]
        public void LikeOnNullableDate(string date, bool result)
        {
            var pattern = "2019";
            var user = new User();
            if (DateTime.TryParse(date, out var parsedDate))
            {
                user.BirthDay = parsedDate;
            }
            var builder = new WebFilterConverter<User>();

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.BirthDay), WebFilterOperand.Like, new List<string> { pattern });
            var expression = builder.ToExpression(filter);
            var compiled = expression.Compile();
            Assert.Equal(result, compiled.Invoke(user));
        }

        [Fact]
        public void Anniversary()
        {
            var builder = new WebFilterConverter<User>();
            
            var goodUser = new User { BirthDay = DateTime.Today };
            var badUser = new User { BirthDay = DateTime.Today.AddDays(1) };
            var nullUser = new User { };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.BirthDay), WebFilterOperand.Anniversary, new List<DateTime?> { DateTime.Today });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, badUser, nullUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void AnniversaryNotNullable()
        {
            var builder = new WebFilterConverter<User>();
            
            var goodUser = new User { ContractStart = DateTime.Today };
            var badUser = new User { ContractStart = DateTime.Today.AddDays(1) };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.ContractStart), WebFilterOperand.Anniversary, new List<DateTime?> { DateTime.Today });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void NullAnniversary()
        {
            var builder = new WebFilterConverter<User>();

            var goodUser = new User { BirthDay = DateTime.Today };
            var badUser = new User { BirthDay = DateTime.Today.AddDays(1) };
            var nullUser = new User { };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.BirthDay), WebFilterOperand.Anniversary, new List<DateTime?> { DateTime.Today, null });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, badUser, nullUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], nullUser);
        }

        [Fact]
        public void UntilOnDatetime()
        {
            var builder = new WebFilterConverter<User>();
            var goodUser = new User { BirthDay = DateTime.Today };
            var badUser = new User { BirthDay = DateTime.Today.AddDays(1) };
            var nullUser = new User { };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.BirthDay), WebFilterOperand.Until, new List<DateTime?> { DateTime.Today, null });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, badUser, nullUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], nullUser);
        }

        [Fact]
        public void SinceOnDatetime()
        {
            var builder = new WebFilterConverter<User>();
            var goodUser = new User { BirthDay = DateTime.Today };
            var badUser = new User { BirthDay = DateTime.Today.AddDays(-1) };
            var nullUser = new User { };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.BirthDay), WebFilterOperand.Since, new List<DateTime?> { DateTime.Today, null });
            var expression = builder.ToExpression(filter);
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
            var goodUser = new User { Id = 1 };
            var badUser = new User { Id = 3 };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.Id), WebFilterOperand.Equals, new List<int>(values));
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void EqualsFilterOnCollection()
        {
            var builder = new WebFilterConverter<User>();
            var goodUser = new User { Users = new List<User> { new User { Id = 20 }, new User { Id = 21 } } };
            var badUser = new User { Users = new List<User> { new User { Id = 30 }, new User { Id = 31 } } };

            var filter = new WebFilter<User>(new ExpressionParser().Parse<User>("users.id"), WebFilterOperand.Equals, new List<int> { 20 });
            var expression = builder.ToExpression(filter);
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

            var goodUser = new User { Id = 3 };
            var badUser = new User { Id = 2 };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.Id), WebFilterOperand.NotEqual, new List<int>(values));
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void ContainsAllFilter()
        {
            var builder = new WebFilterConverter<User>();

            var badUser = new User { Id = 3 };
            var badUser2 = new User { Id = 2 };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.Id), WebFilterOperand.ContainsAll, new List<int> { 1, 2 });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { badUser2, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Empty(users);

            Assert.Throws<QueryBuilderException>(() => builder.ContainsAll(filter.Expression, Enumerable.Range(0, 10000).ToList()));
        }

        [Fact]
        public void GreaterThanOrEqualFilter()
        {
            var builder = new WebFilterConverter<User>();

            var goodUser = new User { Id = 3 };
            var goodUser2 = new User { Id = 1 };
            var badUser = new User { Id = 0 };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.Id), WebFilterOperand.GreaterThanOrEqual, new List<int> { 1, 2 });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, goodUser2, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], goodUser2);
        }

        [Fact]
        public void LessThanOrEqualFilter()
        {
            var builder = new WebFilterConverter<User>();

            var goodUser = new User { Id = 0 };
            var goodUser2 = new User { Id = 1 };
            var badUser = new User { Id = 3 };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.Id), WebFilterOperand.LessThanOrEqual, new List<int> { 1, 2 });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, goodUser2, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], goodUser2);
        }

        [Fact]
        public void GreaterThanFilter()
        {
            var builder = new WebFilterConverter<User>();
            var goodUser = new User { Id = 3 };
            var badUser = new User { Id = 1 };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.Id), WebFilterOperand.GreaterThan, new List<int> { 1, 2 });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void LessThanFilter()
        {
            var builder = new WebFilterConverter<User>();
            var goodUser = new User { Id = 1 };
            var badUser = new User { Id = 2 };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.Id), WebFilterOperand.LessThan, new List<int> { 1, 2 });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void BetweenFilter()
        {
            var builder = new WebFilterConverter<User>();
            var goodUser = new User { ContractStart = DateTime.Today.AddHours(12) };
            var badUser = new User { ContractStart = DateTime.Today.AddDays(2) };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.ContractStart), WebFilterOperand.Between, new List<Period> { new Period(DateTime.Today, DateTime.Today.AddDays(1)) });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Single(users);
            Assert.Equal(users[0], goodUser);
        }

        [Fact]
        public void StartsFilter()
        {
            var builder = new WebFilterConverter<User>();
            var goodUser = new User { Name = "AAAseed" };
            var goodUser2 = new User { Name = "bBbseed" };
            var badUser = new User { Name = "edgseed" };

            var filter = new WebFilter<User>(PropertyExpression<User>.New(u => u.Name), WebFilterOperand.Starts, new List<string> { "aAa", "bbb" });
            var expression = builder.ToExpression(filter);
            var users = new List<User> { goodUser, goodUser2, badUser }.AsQueryable().Where(expression).ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(users[0], goodUser);
            Assert.Equal(users[1], goodUser2);
        }
    }
}