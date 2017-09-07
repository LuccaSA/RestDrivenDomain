using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace RDD.Web.Contexts
{
	public interface IWebContext : IDisposable
	{
		Uri Url { get; }
		string RawUrl { get; }
		IEnumerable<KeyValuePair<string, StringValues>> QueryString { get; }
		IEnumerable<KeyValuePair<string, StringValues>> Headers { get; }
		IEnumerable<KeyValuePair<string, string>> Cookies { get; }
		string GetCookie(string cookieName);
		IDictionary<object, object> Items { get; }
		string ApplicationPath { get; }
		string PhysicalApplicationPath { get; }
		Dictionary<string, string> GetQueryNameValuePairs();
		string UserHostAddress { get; }
	}
}
