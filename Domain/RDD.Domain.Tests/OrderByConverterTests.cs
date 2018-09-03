using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
    public class OrderByConverterTests
    {
        [Fact]
        public void SimpleOrderByShouldWork()
        {
            var orderby = OrderBy<User>.Ascending(u => u.Name);
            var users = new List<User> { new User { Name = "bName" }, new User { Name = "aName" } }.AsQueryable();

            var results = orderby.ApplyOrderBy(users);

            Assert.Equal("aName", results.ElementAt(0).Name);
        }

        [Fact]
        public void SimpleOrdersByShouldWork()
        {
            var orderby1 = OrderBy<User>.Ascending(u => u.Name);
            var orderby2 = OrderBy<User>.Ascending(u => u.Id);
            var users = new List<User>
            {
                new User { Name = "bName", Id = 30},
                new User { Name = "bName", Id = 0},
                new User { Name = "aName" }
            }.AsQueryable();

            var results = orderby1.ApplyOrderBy(users);
            results = orderby2.ApplyOrderBy(results);

            Assert.Equal("aName", results.ElementAt(0).Name);
            Assert.Equal(0, results.ElementAt(0).Id);
        }

        [Fact]
        public void FullOrdersByShouldWork()
        {
            var orderby1 = OrderBy<User>.Ascending(u => u.Name);
            var orderby2 = OrderBy<User>.Ascending(u => u.Mail);
            var orderby3 = OrderBy<User>.Descending(u => u.PictureId);
            var orderby4 = OrderBy<User>.Descending(u => u.Id);
            var users = new List<User>
            {
                new User { Name = "aaa", Id = 4},
                new User { Name = "aaa", Id = 30},
                new User { Name = "aaa", Id = 0},
            }.AsQueryable();

            var results = orderby1.ApplyOrderBy(users);
            results = orderby2.ApplyOrderBy(results);
            results = orderby3.ApplyOrderBy(results);
            results = orderby4.ApplyOrderBy(results);

            Assert.Equal(30, results.ElementAt(0).Id);
        }
    }
}
