using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace RDD.Web.Querying
{
    public static class HttpContextExtension
    {
        public static Dictionary<string, string> GetQueryNameValuePairs(this HttpContext httpContext)
        {
            return httpContext.Request.Query.Where(s => !String.IsNullOrEmpty(s.Key)).ToDictionary(k => k.Key, k => String.Join(",", k.Value.ToArray()));
        }

        public static string GetContent(this HttpContext httpContext)
        {
            string content;
            using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }
            return content;
        }
    }
}