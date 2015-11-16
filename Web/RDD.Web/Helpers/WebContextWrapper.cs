using RDD.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace RDD.Web.Helpers
{
	public class HttpContextWrapper : IWebContext
	{
		private HttpContext _context;

		public HttpContextWrapper(HttpContext context)
		{
			_context = context;
		}

		public Uri Url { get { return _context.Request.Url; } }
		public string RawUrl { get { return _context.Request.RawUrl; } }
		public string HttpMethod { get { return _context.Request.HttpMethod; } }
		public NameValueCollection QueryString { get { return _context.Request.QueryString; } }
		public Stream InputStream { get { return _context.Request.InputStream; } }
		public NameValueCollection Headers { get { return _context.Request.Headers; } }
		public Dictionary<string, string> Cookies { get { return _context.Request.Cookies.AllKeys.ToDictionary(k => k, k => _context.Request.Cookies[k].Value); } }
		public IDictionary Items { get { return _context.Items; } }
		public string ApplicationPath { get { return _context.Request.ApplicationPath; } }
		public string PhysicalApplicationPath { get { return _context.Request.PhysicalApplicationPath; } }
		public Dictionary<string, string> GetQueryNameValuePairs()
		{
			var collection = _context.Request.Url.ParseQueryString();
			return collection.AllKeys.ToDictionary(k => k, k => collection[k]);
		}
		public string UserHostAddress { get { return _context.Request.UserHostAddress; } }
		public string UserHostName { get { return _context.Request.UserHostName; } }
		public string UserAgent { get { return _context.Request.UserAgent; } }
		public string BrowserType { get { return _context.Request.Browser.Type; } }
		public int BrowserMajorVersion { get { return _context.Request.Browser.MajorVersion; } }
		public void Redirect(Uri url, bool endReponse = true)
		{
			_context.Response.Redirect(url.ToString(), endReponse);
		}

		public void Dispose()
		{
		}
	}
}