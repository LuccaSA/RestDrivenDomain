using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Infra.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RDD.Infra.Contexts
{
	public class InMemoryWebContext : IWebContext
	{
		protected Dictionary<string, object> _items;

		public InMemoryWebContext()
		{
			_items = new Dictionary<string, object>();
		}

		public InMemoryWebContext(IDictionary items)
		{
			_items = new Dictionary<string, object>()
			{
				{ "executionContext", items["executionContext"]},
				{ "repoProvider", items["repoProvider"]}
			};
		}

		public Uri Url { get { throw new NotImplementedException(); } }
		public string RawUrl { get { throw new NotImplementedException(); } }
		public string HttpMethod { get { throw new NotImplementedException(); } }
		public NameValueCollection QueryString { get { throw new NotImplementedException(); } }
		public Stream InputStream { get { throw new NotImplementedException(); } }
		public NameValueCollection Headers { get { throw new NotImplementedException(); } }
		public Dictionary<string, string> Cookies { get { throw new NotImplementedException(); } }
		public string GetCookie(string cookieName) { throw new NotImplementedException(); }
		public void SetCookie(string cookieName, string value, DateTime expiration) { throw new NotImplementedException(); }
		public string ApplicationPath { get { throw new NotImplementedException(); } }
		public string PhysicalApplicationPath { get { throw new NotImplementedException(); } }
		public Dictionary<string, string> GetQueryNameValuePairs() { throw new NotImplementedException(); }
		public string UserHostAddress { get { throw new NotImplementedException(); } }
		public string UserHostName { get { throw new NotImplementedException(); } }
		public string UserAgent { get { throw new NotImplementedException(); } }
		public string BrowserType { get { throw new NotImplementedException(); } }
		public int BrowserMajorVersion { get { throw new NotImplementedException(); } }
		public void Redirect(Uri url, bool endResponse) { throw new NotImplementedException(); }

		public IDictionary Items { get { return _items; } }

		public void Dispose()
		{
		}
	}
}