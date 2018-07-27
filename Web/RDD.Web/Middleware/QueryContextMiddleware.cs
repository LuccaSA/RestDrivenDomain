using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Middleware
{
    public class QueryContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly QueryMetadata _queryMetadata;

        public QueryContextMiddleware(RequestDelegate next, QueryMetadata queryMetadata)
        {
            _next = next;
            _queryMetadata = queryMetadata;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);
        }
    }
}
