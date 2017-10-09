using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using NExtends.Primitives;
using RDD.Infra.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RDD.Infra.Contexts
{
    public class HttpContextWrapper : IWebContextWrapper
    {
        public Uri Url { get; private set; }
        public string RawUrl { get; private set; }
        public string HttpMethod { get; private set; }
        public Dictionary<object, object> Items { get; private set; }
        IDictionary<object, object> IWebContext.Items => Items;
        public IEnumerable<KeyValuePair<string, StringValues>> QueryString { get; private set; }
        public IEnumerable<KeyValuePair<string, StringValues>> Headers { get; private set; }
        public Dictionary<string, string> Cookies { get; private set; }
        IEnumerable<KeyValuePair<string, string>> IWebContext.Cookies => Cookies;
        public string ApplicationPath { get; private set; }
        public string PhysicalApplicationPath { get; private set; }
        public string UserHostAddress { get; private set; }
        public string Content { get; private set; }
        public string ContentType { get; private set; }

        public HttpContextWrapper() { }

        public void SetContext(HttpContext context)
        {
            Url = new Uri(context.Request.GetDisplayUrl());
            RawUrl = context.Request.GetDisplayUrl();
            HttpMethod = context.Request.Method;
            Items = new Dictionary<object, object>(context.Items);
            QueryString = context.Request.Query;
            Headers = context.Request.Headers;
            Cookies = context.Request.Cookies.ToDictionary();
            ApplicationPath = context.Request.Path;
            PhysicalApplicationPath = context.Request.PathBase.Value;
            UserHostAddress = context.Connection.RemoteIpAddress?.ToString();
            Content = GetContent(context.Request.Body);
            ContentType = context.Request.ContentType;
        }

        private string GetContent(Stream body)
        {
            string content;
            using (var reader = new StreamReader(body, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }
            return content;
        }

        public Dictionary<string, string> GetQueryNameValuePairs()
        {
            return QueryString.Where(s => !String.IsNullOrEmpty(s.Key)).ToDictionary(k => k.Key, k => String.Join(",", k.Value.ToArray()));
        }

        public string GetCookie(string cookieName)
        {
            return Cookies.ContainsKey(cookieName) ? Cookies[cookieName] : null;
        }

        public void Dispose()
        {
            var threadID = Thread.CurrentThread.ManagedThreadId;
            if (AsyncService.ThreadedContexts.ContainsKey(threadID))
            {
                IWebContext context;

                AsyncService.ThreadedContexts.TryRemove(threadID, out context);
            }
        }
    }
}