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
                new User { Name = "aName", Id = 35 }
            }.AsQueryable();

            var results = orderby1.ApplyOrderBy(users);
            results = orderby2.ApplyOrderBy(results);

            Assert.Equal(35, results.ElementAt(0).Id);
            Assert.Equal(0, results.ElementAt(1).Id);
            Assert.Equal(30, results.ElementAt(2).Id);
        }

        [Fact]
        public void FullOrdersByShouldWork()
        {
            var orderby1 = OrderBy<User>.Ascending(u => u.Name);
            var orderby2 = OrderBy<User>.Ascending(u => u.Salary);
            var orderby3 = OrderBy<User>.Descending(u => u.Id);

            var users = new List<User>
            {
                new User { Name = "b", Id = 26, Salary = 30},
                new User { Name = "b", Id = 27, Salary = 0},
                new User { Name = "b", Id = 28, Salary = 30},
                new User { Name = "a", Id = 29},
                new User { Name = "a", Id = 30},
            }.AsQueryable();

            var results = orderby1.ApplyOrderBy(users);
            results = orderby2.ApplyOrderBy(results);
            results = orderby3.ApplyOrderBy(results);

            Assert.Equal(30, results.ElementAt(0).Id);
            Assert.Equal(29, results.ElementAt(1).Id);
            Assert.Equal(27, results.ElementAt(2).Id);
            Assert.Equal(28, results.ElementAt(3).Id);
            Assert.Equal(26, results.ElementAt(4).Id);
        }
    }
}
