using Newtonsoft.Json.Serialization;
using RDD.Domain.Helpers;
using RDD.Web.Helpers;
using RDD.Web.Tests.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace RDD.Web.Tests
{
    public class ApiHelperTests
    {
        [Fact]
        public void ConvertingFiltersShouldKeepTheEntityType()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            httpContextAccessor.HttpContext.Request.QueryString = QueryString.Create("id", "2");
            var helper = new ApiHelper<User, int>(httpContextAccessor ,null, null);
            var query = helper.CreateQuery(HttpVerbs.Get);

            Assert.Single(query.Filters);

            var filter = query.Filters.ElementAt(0);

            Assert.Equal(typeof(PropertySelector<User>), filter.Property.GetType());

            Assert.True(filter.Property.Contains(u => u.Id));
        }
    }
}
