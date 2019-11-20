using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Rdd.Domain.Helpers;
using System;
using System.Linq;

namespace Rdd.Web.Tests
{
    public static class HttpRequestHelper
    {
        public static HttpRequest NewRequest(this HttpVerbs httpVerb, params (string, string)[] values)
        {
            var httpContext = new DefaultHttpContext();
            switch (httpVerb)
            {
                case HttpVerbs.Get:
                    httpContext.Request.Method = HttpMethods.Get;
                    break;
                case HttpVerbs.Post:
                    httpContext.Request.Method = HttpMethods.Post;
                    break;
                case HttpVerbs.Put:
                    httpContext.Request.Method = HttpMethods.Put;
                    break;
                case HttpVerbs.Delete:
                    httpContext.Request.Method = HttpMethods.Delete;
                    break;
                case HttpVerbs.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpVerb), httpVerb, null);
            }

#if NETCOREAPP2_2
            httpContext.Request.Query = new Microsoft.AspNetCore.Http.Internal.QueryCollection(values.ToDictionary(i => i.Item1, i => (StringValues)i.Item2));
#endif
#if NETCOREAPP3_0
            httpContext.Request.Query = new QueryCollection(values.ToDictionary(i => i.Item1, i => (StringValues)i.Item2));
#endif
            return httpContext.Request;
        }
    }
}