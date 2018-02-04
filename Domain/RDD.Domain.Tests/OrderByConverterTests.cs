using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Models.Querying.Convertors;
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
            var orderby = new OrderBy<User>(u => u.Name);
            var orderBys = new Queue<OrderBy<User>>(new List<OrderBy<User>> { orderby });
            var converter = new OrderBysConverter<User>();
            var users = new List<User> { new User { Name = "bName" }, new User { Name = "aName" } }.AsQueryable();

            var results = converter.Convert(users, orderBys);

            Assert.Equal("aName", results.ElementAt(0).Name);
        }
    }
}
