using Newtonsoft.Json.Serialization;
using RDD.Domain.Helpers;
using RDD.Web.Helpers;
using RDD.Web.Tests.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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
            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = QueryString.Create("pictureId", "like,aabbccdd-eeff");

            QueryFactoryHelper.NewQueryFactory(httpContext)
                .NewFromHttpRequest<User, int>(HttpVerbs.Get);
        }

        [Fact]
        public void FilterKeywordsShouldBeCaseInsensitive()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = QueryString.Create("name", "NOTEQUAL,foo");
            
            QueryFactoryHelper.NewQueryFactory(httpContext)
                .NewFromHttpRequest<User, int>(HttpVerbs.Get);
        }
         
    }
}
