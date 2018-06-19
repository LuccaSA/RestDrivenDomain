using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Helpers
{

    internal static class HttpContextExtensions
    {
        private static readonly object _queryKey = new object();

        public static T GetService<T>(this HttpContext httpContext)
        {
            return (T)httpContext.RequestServices.GetService(typeof(T));
        }

        public static Query GetContextualQuery(this HttpContext httpContext)
        {
            return httpContext.Items[_queryKey] as Query;
        }

        public static void SetContextualQuery(this HttpContext httpContext, Query query)
        {
            httpContext.Items[_queryKey] = query;
        }
    }
}
