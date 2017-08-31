using Microsoft.Extensions.Primitives;
using RDD.Domain;
using RDD.Infra.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RDD.Infra.Contexts
{
	public class InMemoryWebContext : IWebContext
	{
		protected Dictionary<object, object> _items;

		public InMemoryWebContext()
		{
			_items = new Dictionary<object, object>();
		}

		public InMemoryWebContext(IDictionary<object, object> items)
			: this()
		{
			foreach (var kvp in items)
			{
				_items.Add(kvp.Key, kvp.Value);
			}
		}

		public Uri Url { get { throw new NotImplementedException(); } }
		public string RawUrl { get { throw new NotImplementedException(); } }
		public string HttpMethod { get { throw new NotImplementedException(); } }
		public IEnumerable<KeyValuePair<string, StringValues>> QueryString { get { throw new NotImplementedException(); } }
		public IEnumerable<KeyValuePair<string, StringValues>> Headers { get { throw new NotImplementedException(); } }
		public IEnumerable<KeyValuePair<string, string>> Cookies { get { throw new NotImplementedException(); } }
		public string GetCookie(string cookieName) { throw new NotImplementedException(); }
		public string ApplicationPath { get { throw new NotImplementedException(); } }
		public string PhysicalApplicationPath { get { throw new NotImplementedException(); } }
		public Dictionary<string, string> GetQueryNameValuePairs() { throw new NotImplementedException(); }
		public string UserHostAddress { get { throw new NotImplementedException(); } }
		public void Redirect(Uri url, bool endResponse) { throw new NotImplementedException(); }

		public IDictionary<object, object> Items { get { return _items; } }

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