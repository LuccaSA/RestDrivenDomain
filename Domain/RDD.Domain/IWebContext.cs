using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RDD.Domain
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
		void Redirect(Uri url, bool endResponse);
	}
}
