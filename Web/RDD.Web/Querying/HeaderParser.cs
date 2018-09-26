using System;
using Microsoft.AspNetCore.Http;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Querying
{
    public class HeaderParser : IHeaderParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeaderParser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual Headers ParseHeaders()
        {
            var headers = new Headers
            {
                RawHeaders = _httpContextAccessor.HttpContext.Request.Headers
            };

            foreach (var element in headers.RawHeaders)
            {
                switch (element.Key)
                {
                    case "If-Unmodified-Since":
                        if (DateTime.TryParse(element.Value, out DateTime unModifiedSince))
                        {
                            headers.IfUnmodifiedSince = unModifiedSince;
                        }
                        break;
                    case "Authorization":
                        headers.Authorization = element.Value;
                        break;
                    case "Content-Type":
                        headers.ContentType = element.Value;
                        break;
                }
            }

            return headers;
        }
    }
}