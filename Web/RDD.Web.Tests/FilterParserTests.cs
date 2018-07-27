using Newtonsoft.Json.Serialization;
using RDD.Domain.Helpers;
using RDD.Web.Helpers;
using RDD.Web.Tests.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using RDD.Domain.Models.Querying;
using RDD.Web.Querying;
using Xunit;

namespace RDD.Web.Tests
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
            var helper = new ApiHelper<User, int>(httpContextHelper, QueryFactory);

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
            var helper = new ApiHelper<User, int>(httpContextHelper, QueryFactory);

            helper.CreateQuery(HttpVerbs.Get);
        }

        private QueryFactory QueryFactory => new QueryFactory
        (
            new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            }, new QueryTokens(), new QueryMetadata()
        );
    }
}
