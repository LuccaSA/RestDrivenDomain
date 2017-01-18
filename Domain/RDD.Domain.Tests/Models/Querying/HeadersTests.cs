using NUnit.Framework;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests.Models.Querying
{
	public class HeadersTests
	{
		[Test]
		public void Headers_ShouldParseIfUnmodifiedSinceHeader()
		{
			var date = new DateTime(2015, 9, 21, 7, 28, 0);
			var requestHeaders = new NameValueCollection { { "If-Unmodified-Since", date.ToString("ddd, dd MMM yyyy HH:mm:ss zzz") } };

			var headers = Headers.Parse(requestHeaders);

			Assert.AreEqual(0, DateTime.Compare(date, headers.IfUnmodifiedSince));
		}

		[Test]
		public void Headers_ShouldParseAuthorizationHeader()
		{
			var authorization = "anything here";
			var requestHeaders = new NameValueCollection { { "Authorization", authorization } };

			var headers = Headers.Parse(requestHeaders);

			Assert.AreEqual(authorization, headers.Authorization);
		}

		[Test]
		public void Headers_ShouldParseContentTypeHeader()
		{
			var contentType = "multipart/form-data";
			var requestHeaders = new NameValueCollection { { "Content-Type", contentType } };

			var headers = Headers.Parse(requestHeaders);

			Assert.AreEqual(contentType, headers.ContentType);
		}

		[Test]
		public void Headers_ShouldParseRawHeaders()
		{
			var requestHeaders = new NameValueCollection
			{
				{ "Content-Type", "multipart/form-data" },
				{ "any", "value" },
				{ "Foo", "Bar" },
			};

			var headers = Headers.Parse(requestHeaders);

			Assert.AreEqual(3, headers.RawHeaders.Count);

			Assert.AreEqual("multipart/form-data", headers.RawHeaders["Content-Type"]);
			Assert.AreEqual("value", headers.RawHeaders["any"]);
			Assert.AreEqual("Bar", headers.RawHeaders["Foo"]);
		}
	}
}
