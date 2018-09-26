using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Web.Querying;

namespace RDD.Web.Tests
{
    public static class QueryFactoryHelper
    {
        public static QueryFactory NewQueryFactory(HttpContext httpContext = null, PagingOptions rddOptions = null)
        {
            return new QueryFactory
            (
                new QueryMetadata(),
                NewQueryParsers(httpContext, rddOptions)
            );
        }

        public static QueryParsers NewQueryParsers(HttpContext httpContext = null, PagingOptions rddOptions = null)
        {
            var httpContextAccessor = new HttpContextAccessor()
            {
                HttpContext = httpContext ?? new DefaultHttpContext()
            };

            return new QueryParsers(
                new WebFilterParser(new QueryTokens(), httpContextAccessor),
                new PagingParser(httpContextAccessor, Options.Create(rddOptions ?? new PagingOptions())),
                new HeaderParser(httpContextAccessor),
                new OrderByParser(httpContextAccessor),
                new FieldsParser(httpContextAccessor)
                );
        }

    }
}