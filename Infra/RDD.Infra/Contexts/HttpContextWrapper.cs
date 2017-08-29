using RDD.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace RDD.Infra.Contexts
{
	public class HttpContextWrapper : IWebContext
	{
		private HttpContext _context;

		public HttpContextWrapper(HttpContext context)
		{
			_context = context;
		}

		public Uri Url { get { return new Uri(_context.Request.GetDisplayUrl()); } }
		public string RawUrl { get { return _context.Request.GetDisplayUrl(); } }
		public string HttpMethod { get { return _context.Request.Method; } }
		public IEnumerable<KeyValuePair<string, StringValues>> QueryString { get { return _context.Request.Query; } }
		public IEnumerable<KeyValuePair<string, StringValues>> Headers { get { return _context.Request.Headers; } }
	public IEnumerable<KeyValuePair<string, string>> Cookies { get { return _context.Request.Cookies; } }
		public string GetCookie(string cookieName) { return _context.Request.Cookies.ContainsKey(cookieName) ? _context.Request.Cookies[cookieName] : null; }
		public IDictionary<object, object> Items { get { return _context.Items; } }
		public string ApplicationPath { get { return _context.Request.Path; } }
		public string PhysicalApplicationPath { get { return _context.Request.PathBase.Value; } }
		public Dictionary<string, string> GetQueryNameValuePairs()
		{
			return QueryString.ToDictionary(k => k.Key, k => String.Join(",", k.Value.ToArray()));
		}
		public string UserHostAddress { get { return _context.Connection.RemoteIpAddress?.ToString(); } }
		public string Content
		{
			get
			{
				string content;
				using (var reader = new StreamReader(_context.Request.Body, Encoding.UTF8))
				{
					content = reader.ReadToEnd();
				}
				return content;
			}
		}
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