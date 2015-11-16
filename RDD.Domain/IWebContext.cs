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
		NameValueCollection QueryString { get; }
		NameValueCollection Headers { get; }	
		Dictionary<string, string> Cookies { get; }
		IDictionary Items { get; }
		string ApplicationPath { get; }
		string PhysicalApplicationPath { get; }
		Dictionary<string, string> GetQueryNameValuePairs();
		string UserHostAddress { get; }
		string UserAgent { get; }
		string BrowserType { get; }
		int BrowserMajorVersion { get; }
		void Redirect(Uri url, bool endResponse);
	}
}
