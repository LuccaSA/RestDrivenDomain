using Rdd.Domain.Helpers;
using Rdd.Web.Helpers;
using Rdd.Web.Tests.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Rdd.Web.Querying;
using Xunit;

namespace Rdd.Web.Tests
{
    public class FilterParserTests
    {
        [Fact]
        public void LikeOperationOnGuidShouldWork()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            httpContextAccessor.HttpContext.Request.QueryString = QueryString.Create("pictureId", "like,aabbccdd-eeff");
            var httpContextHelper = new HttpContextHelper(httpContextAccessor);
            var options = Options.Create(new RddOptions());

            var helper = new ApiHelper<User, int>(httpContextHelper, new WebQueryFactory<User, int>(options));

            helper.CreateQuery(HttpVerbs.Get);
        }

        [Fact]
        public void FilterKeywordsShouldBeCaseInsensitive()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            httpContextAccessor.HttpContext.Request.QueryString = QueryString.Create("name", "NOTEQUAL,foo");
            var httpContextHelper = new HttpContextHelper(httpContextAccessor);
            var options = Options.Create(new RddOptions());

            var helper = new ApiHelper<User, int>(httpContextHelper, new WebQueryFactory<User, int>(options));

            helper.CreateQuery(HttpVerbs.Get);
        }
    }
}
