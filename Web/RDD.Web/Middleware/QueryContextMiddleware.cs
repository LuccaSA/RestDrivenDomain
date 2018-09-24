using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Middleware
{
    public class QueryContextMiddleware
    {
        private readonly RequestDelegate _next;

        public QueryContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context, QueryMetadata queryMetadata)
        {
            queryMetadata.StartWatch();
            return _next(context);
        }
    }
}
