using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Serialization;
using RDD.Domain.Helpers;
using RDD.Infra.Contexts;
using RDD.Web.Helpers;
using RDD.Web.Tests.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RDD.Web.Tests
{
    public class ApiHelperTests
    {
        [Fact]
        public void ConvertingFiltersShouldKeepTheEntityType()
        {
            var webContext = new InMemoryWebContext();
            webContext.QueryString = new Dictionary<string, StringValues>() { { "id", "2" } };
            webContext.Headers = new Dictionary<string, StringValues>();

            var helper = new ApiHelper<User, int>(new CamelCasePropertyNamesContractResolver(), webContext, null, null);
            var query = helper.CreateQuery(HttpVerbs.Get);

            Assert.Single(query.Filters);

            var filter = query.Filters.ElementAt(0);

            Assert.Equal(typeof(PropertySelector<User>), filter.Property.GetType());

            Assert.True(filter.Property.Contains(u => u.Id));
        }
    }
}
