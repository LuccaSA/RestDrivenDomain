using Microsoft.Extensions.Primitives;
using RDD.Domain.Models.Querying;
using NExtends.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xunit;
using RDD.Web.Querying;

namespace RDD.Domain.Tests.Models.Querying
{
	public class HeadersTests
	{
		[Fact]
		public void Headers_ShouldParseIfUnmodifiedSinceHeader()
		{
			var date = new DateTime(2015, 9, 21, 7, 28, 0);
			var requestHeaders = new Dictionary<string, StringValues> { { "If-Unmodified-Since", date.ToString("ddd, dd MMM yyyy HH:mm:ss zzz") } };

			var headers = new HeadersParser().Parse(requestHeaders);

			Assert.True(headers.IfUnmodifiedSince.HasValue);
			Assert.Equal(0, DateTime.Compare(date, headers.IfUnmodifiedSince.Value));
		}

		[Fact]
		public void Headers_ShouldNotParseIfUnmodifiedSinceHeader_WhenDateFormatIsInvalid()
		{
			var requestHeaders = new Dictionary<string, StringValues> { { "If-Unmodified-Since", "invalid format" } };

			var headers = new HeadersParser().Parse(requestHeaders);

			Assert.Equal(false, headers.IfUnmodifiedSince.HasValue);
		}

		[Fact]
		public void Headers_ShouldParseAuthorizationHeader()
		{
			var authorization = "anything here";
			var requestHeaders = new Dictionary<string, StringValues> { { "Authorization", authorization } };

			var headers = new HeadersParser().Parse(requestHeaders);

			Assert.Equal(authorization, headers.Authorization);
		}

		[Fact]
		public void Headers_ShouldParseContentTypeHeader()
		{
			var contentType = "multipart/form-data";
			var requestHeaders = new Dictionary<string, StringValues> { { "Content-Type", contentType } };

			var headers = new HeadersParser().Parse(requestHeaders);

			Assert.Equal(contentType, headers.ContentType);
		}

		[Fact]
		public void Headers_ShouldParseRawHeaders()
		{
			var requestHeaders = new Dictionary<string, StringValues>
			{
				{ "Content-Type", "multipart/form-data" },
				{ "any", "value" },
				{ "Foo", "Bar" },
			};

			var rawHeaders = new HeadersParser().Parse(requestHeaders).RawHeaders.ToDictionary();

			Assert.Equal(3, rawHeaders.Count);

			Assert.Equal("multipart/form-data", rawHeaders["Content-Type"]);
			Assert.Equal("value", rawHeaders["any"]);
			Assert.Equal("Bar", rawHeaders["Foo"]);
		}
	}
}
