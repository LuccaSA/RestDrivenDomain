using RDD.Domain.Models.Querying;
using System;
using System.Collections.Specialized;
using Xunit;

namespace RDD.Domain.Tests.Models.Querying
{
	public class HeadersTests
	{
		[Fact]
		public void Headers_ShouldParseIfUnmodifiedSinceHeader()
		{
			var date = new DateTime(2015, 9, 21, 7, 28, 0);
			var requestHeaders = new NameValueCollection { { "If-Unmodified-Since", date.ToString("ddd, dd MMM yyyy HH:mm:ss zzz") } };

			var headers = Headers.Parse(requestHeaders);

			Assert.Equal(true, headers.IfUnmodifiedSince.HasValue);
			Assert.Equal(0, DateTime.Compare(date, headers.IfUnmodifiedSince.Value));
		}

		[Fact]
		public void Headers_ShouldNotParseIfUnmodifiedSinceHeader_WhenDateFormatIsInvalid()
		{
			var requestHeaders = new NameValueCollection { { "If-Unmodified-Since", "invalid format" } };

			var headers = Headers.Parse(requestHeaders);

			Assert.Equal(false, headers.IfUnmodifiedSince.HasValue);
		}

		[Fact]
		public void Headers_ShouldParseAuthorizationHeader()
		{
			var authorization = "anything here";
			var requestHeaders = new NameValueCollection { { "Authorization", authorization } };

			var headers = Headers.Parse(requestHeaders);

			Assert.Equal(authorization, headers.Authorization);
		}

		[Fact]
		public void Headers_ShouldParseContentTypeHeader()
		{
			var contentType = "multipart/form-data";
			var requestHeaders = new NameValueCollection { { "Content-Type", contentType } };

			var headers = Headers.Parse(requestHeaders);

			Assert.Equal(contentType, headers.ContentType);
		}

		[Fact]
		public void Headers_ShouldParseRawHeaders()
		{
			var requestHeaders = new NameValueCollection
			{
				{ "Content-Type", "multipart/form-data" },
				{ "any", "value" },
				{ "Foo", "Bar" },
			};

			var headers = Headers.Parse(requestHeaders);

			Assert.Equal(3, headers.RawHeaders.Count);

			Assert.Equal("multipart/form-data", headers.RawHeaders["Content-Type"]);
			Assert.Equal("value", headers.RawHeaders["any"]);
			Assert.Equal("Bar", headers.RawHeaders["Foo"]);
		}
	}
}
