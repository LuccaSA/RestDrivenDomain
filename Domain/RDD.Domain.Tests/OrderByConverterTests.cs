using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rdd.Domain.Tests
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
            var orderby2 = OrderBy<User>.Ascending(u => u.Salary);
            var id30 = Guid.NewGuid();
            var id0 = Guid.NewGuid();
            var id35 = Guid.NewGuid();
            var users = new List<User>
            {
                new User { Name = "bName", Id = id30, Salary = 30},
                new User { Name = "bName", Id = id0, Salary = 0},
                new User { Name = "aName", Id = id35, Salary = 35 }
            }.AsQueryable();

            var results = orderby1.ApplyOrderBy(users);
            results = orderby2.ApplyOrderBy(results);

            Assert.Equal(id35, results.ElementAt(0).Id);
            Assert.Equal(id0, results.ElementAt(1).Id);
            Assert.Equal(id30, results.ElementAt(2).Id);
        }

        [Fact]
        public void FullOrdersByShouldWork()
        {
            var orderby1 = OrderBy<User>.Ascending(u => u.Name);
            var orderby2 = OrderBy<User>.Ascending(u => u.Salary);
            var orderby3 = OrderBy<User>.Descending(u => u.Id);

            var id26 = new Guid("26000000-0000-0000-0000-000000000000");
            var id27 = new Guid("27000000-0000-0000-0000-000000000000");
            var id28 = new Guid("28000000-0000-0000-0000-000000000000");
            var id29 = new Guid("29000000-0000-0000-0000-000000000000");
            var id30 = new Guid("30000000-0000-0000-0000-000000000000");

            var users = new List<User>
            {
                new User { Name = "b", Id = id26, Salary = 30},
                new User { Name = "b", Id = id27, Salary = 0},
                new User { Name = "b", Id = id28, Salary = 30},
                new User { Name = "a", Id = id29},
                new User { Name = "a", Id = id30},
            }.AsQueryable();

            var results = orderby1.ApplyOrderBy(users);
            results = orderby2.ApplyOrderBy(results);
            results = orderby3.ApplyOrderBy(results);

            Assert.Equal(id30, results.ElementAt(0).Id);
            Assert.Equal(id29, results.ElementAt(1).Id);
            Assert.Equal(id27, results.ElementAt(2).Id);
            Assert.Equal(id28, results.ElementAt(3).Id);
            Assert.Equal(id26, results.ElementAt(4).Id);
        }

        [Fact]
        public void OrderByToStringShouldWork()
        {
            var orderby = OrderBy<User>.Ascending(u => u.Id.ToString());
            var users = User.GetManyRandomUsers(10).AsQueryable();

            var orderedIds = users.OrderBy(u => u.Id.ToString()).Select(i => i.Id).ToList();            
            var result = orderby.ApplyOrderBy(users);
            Assert.Equal(orderedIds, result.Select(i => i.Id).ToList());
        }
    }
}
