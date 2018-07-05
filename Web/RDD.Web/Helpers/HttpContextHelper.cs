using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RDD.Web.Helpers
{
    public class HttpContextHelper : IHttpContextHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetContentType()
        {
            return _httpContextAccessor.HttpContext.Request.ContentType.Split(';')[0];
        }

        public Dictionary<string, string> GetQueryNameValuePairs()
        {
            return _httpContextAccessor.HttpContext.Request.Query.Where(s => !String.IsNullOrEmpty(s.Key)).ToDictionary(k => k.Key, k => String.Join(",", k.Value.ToArray()));
        }

        public string GetContent()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            string content;
            using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }
            return content;
        }

        public IDictionary<string, StringValues> GetHeaders()
        {
            return _httpContextAccessor.HttpContext.Request.Headers;
        }
    }
}
