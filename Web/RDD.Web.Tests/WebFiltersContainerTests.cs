using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Infra.Web.Models;
using RDD.Web.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace RDD.Web.Tests
{
    public class WebFiltersContainerTests
    {
        [Fact]
        public void NoFilterShouldWork()
        {
            var filters = new HashSet<WebFilter<User>>();
            var container = new WebFiltersContainer<User, int>(filters);
            Expression<Func<User, object>> expression = u => u.Name;

            Assert.False(container.HasFilter(expression));
        }

        [Fact]
        public void GoodFilterShouldWork()
        {
            var filters = new HashSet<WebFilter<User>>()
            {
                new WebFilter<User>(ExpressionSelectorChain<User>.New(u => u.Name), WebFilterOperand.Equals, new List<string> { "Foo" })
            };
            var container = new WebFiltersContainer<User, int>(filters);
            Expression<Func<User, object>> expression = u => u.Name;

            Assert.True(container.HasFilter(expression));
        }

        [Fact]
        public void BadFilterShouldWork()
        {
            var filters = new HashSet<WebFilter<User>>()
            {
                new WebFilter<User>(ExpressionSelectorChain<User>.New(u => u.Name), WebFilterOperand.Equals, new List<string> { "Foo" })
            };
            var container = new WebFiltersContainer<User, int>(filters);
            Expression<Func<User, object>> expression = u => u.TwitterUri;

            Assert.False(container.HasFilter(expression));
        }

        [Fact]
        public void NullFilterShouldWork()
        {
            var filters = new HashSet<WebFilter<User>>()
            {
                new WebFilter<User, string>(ExpressionSelectorChain<User>.New(u => u.Name), WebFilterOperand.Equals, null)
            };
            var container = new WebFiltersContainer<User, int>(filters);
            Expression<Func<User, object>> expression = u => u.Name;

            Assert.True(container.HasFilter(expression));

            var user1 = new User { Name = "Foo" };
            var user2 = new User { Name = null };

            var users = new HashSet<User> { user1, user2 };
            var compiled = container.Expression.Compile();
            Assert.Single(users.Where(compiled));
        }

        [Fact]
        public void SecondLevelGoodFilterShouldWork()
        {
            var filters = new HashSet<WebFilter<User>>()
            {
                new WebFilter<User>(ExpressionSelectorChain<User>.New(u => u.MyValueObject.Name), WebFilterOperand.Equals, new List<string> { "Foo" })
            };
            var container = new WebFiltersContainer<User, int>(filters);
            Expression<Func<User, object>> expression = u => u.MyValueObject.Name;

            Assert.True(container.HasFilter(expression));
        }

        [Fact]
        public void NeastedFilterWithFirstLevelCheckShouldWork()
        {
            var filters = new HashSet<WebFilter<User>>()
            {
                new WebFilter<User>(ExpressionSelectorChain<User>.New(u => u.MyValueObject.Name), WebFilterOperand.Equals, new List<string> { "Foo" })
            };
            var container = new WebFiltersContainer<User, int>(filters);
            Expression<Func<User, object>> expression = u => u.MyValueObject;

            Assert.True(container.HasFilter(expression));
        }

        [Fact]
        public void FirstLevelFilterWithSecondLevelCheckShouldWork()
        {
            var filters = new HashSet<WebFilter<User>>()
            {
                new WebFilter<User>(ExpressionSelectorChain<User>.New(u => u.MyValueObject), WebFilterOperand.Equals, new List<MyValueObject> { null })
            };
            var container = new WebFiltersContainer<User, int>(filters);
            Expression<Func<User, object>> expression = u => u.MyValueObject.Name;

            Assert.False(container.HasFilter(expression));
        }
    }
}
