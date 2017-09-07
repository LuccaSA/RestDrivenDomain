using Microsoft.Extensions.Primitives;
using NExtends.Primitives;
using RDD.Web.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Tests
{
	public class InMemoryWebContext : IWebContext
	{
		public Uri Url { get; set; }
		public string RawUrl { get; set; }
		public string HttpMethod { get; set; }
		public Dictionary<object, object> Items { get; set; }
		IDictionary<object, object> IWebContext.Items { get { return Items; } }
		public IEnumerable<KeyValuePair<string, StringValues>> QueryString { get; set; }
		public IEnumerable<KeyValuePair<string, StringValues>> Headers { get; set; }
		public Dictionary<string, string> Cookies { get; set; }
		IEnumerable<KeyValuePair<string, string>> IWebContext.Cookies { get { return Cookies; } }
		public string ApplicationPath { get; set; }
		public string PhysicalApplicationPath { get; set; }
		public string UserHostAddress { get; set; }
		public string Content { get; set; }
		public string ContentType { get; set; }
		public Dictionary<string, string> ContentAsFormDictionnary { get; set; }

		public Dictionary<string, string> GetQueryNameValuePairs()
		{
			return QueryString.ToDictionary(k => k.Key, k => String.Join(",", k.Value.ToArray()));
		}

		public string GetCookie(string cookieName)
		{
			return Cookies.ContainsKey(cookieName) ? Cookies[cookieName] : null;
		}

		public void Dispose() { }
	}
}