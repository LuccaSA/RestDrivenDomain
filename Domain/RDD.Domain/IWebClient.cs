using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	// http://brunov.info/blog/2013/07/30/tdd-mocking-system-net-webclient/
	public interface IWebClient : IDisposable
	{
		// Required properties
		RequestCachePolicy CachePolicy { get; set; }
		WebHeaderCollection Headers { get; set; }
		Encoding Encoding { get; set; }

		// Required methods (subset of `System.Net.WebClient` methods).
		string DownloadString(string address);
		string UploadString(string address, string method);
		string UploadString(string address, string method, string data);

		byte[] DownloadData(string address);
		byte[] UploadFile(string address, string method, string fileName);
		byte[] UploadData(string address, string method, byte[] data);
		byte[] UploadValues(string address, string method, NameValueCollection data);
	}
}
