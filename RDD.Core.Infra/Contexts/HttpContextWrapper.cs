using RDD.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace RDD.Infra.Contexts
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
		public string GetCookie(string cookieName) { return Cookies.ContainsKey(cookieName) ? Cookies[cookieName] : null; }
		public void SetCookie(string cookieName, string value, DateTime expiration)
		{
			var cookie = new HttpCookie(cookieName, value);
			cookie.Expires = expiration;

			_context.Response.Cookies.Set(cookie);
		}
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
		public string Content { get { return new StreamReader(_context.Request.InputStream).ReadToEnd(); } }
		public string ContentType { get { return _context.Request.ContentType; } }
		public Dictionary<string, string> ContentAsFormDictionnary
		{
			get
			{
				var content = Content;

				return content.Split('&').Select(s => s.Split('=')).ToDictionary(p => p[0], p => p[1]);
			}
		}

		public void Redirect(Uri url, bool endReponse = true)
		{
			_context.Response.Redirect(url.ToString(), endReponse);
		}

		public void Dispose()
		{
		}
	}
}