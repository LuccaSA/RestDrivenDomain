using Microsoft.Extensions.Primitives;
using NExtends.Primitives.Generics;
using RDD.Web.Querying;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using RDD.Domain.Models.Querying;
using Xunit;

namespace RDD.Domain.Tests.Models.Querying
{
    public class HeadersTests
    {
        private Headers Fakeheaders(string key, string value)
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[key] = value;
            return QueryFactory.ParseHeaders(context.Request);
        }

        [Fact]
        public void Headers_ShouldParseIfUnmodifiedSinceHeader()
        {
            var date = new DateTime(2015, 9, 21, 7, 28, 0);
            var headers = Fakeheaders("If-Unmodified-Since",date.ToString("ddd, dd MMM yyyy HH:mm:ss zzz"));

            Assert.True(headers.IfUnmodifiedSince.HasValue);
            Assert.Equal(0, DateTime.Compare(date, headers.IfUnmodifiedSince.Value));
        }

        [Fact]
        public void Headers_ShouldNotParseIfUnmodifiedSinceHeader_WhenDateFormatIsInvalid()
        {
            var headers = Fakeheaders("If-Unmodified-Since", "invalid format");

            Assert.False(headers.IfUnmodifiedSince.HasValue);
        }

        [Fact]
        public void Headers_ShouldParseAuthorizationHeader()
        {
            var authorization = "anything here";
            var headers = Fakeheaders("Authorization", authorization);

            Assert.Equal(authorization, headers.Authorization);
        }

        [Fact]
        public void Headers_ShouldParseContentTypeHeader()
        {
            var contentType = "multipart/form-data"; 
            var headers = Fakeheaders("Content-Type", contentType); 

            Assert.Equal(contentType, headers.ContentType);
        }

        [Fact]
        public void Headers_ShouldParseRawHeaders()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Content-Type"] = "multipart/form-data";
            context.Request.Headers["any"] = "value";
            context.Request.Headers["Foo"] = "Bar";

            var headers = QueryFactory.ParseHeaders(context.Request);
            var rawHeaders = headers.RawHeaders.ToDictionary();

            Assert.Equal(3, rawHeaders.Count);
            Assert.Equal("multipart/form-data", rawHeaders["Content-Type"]);
            Assert.Equal("value", rawHeaders["any"]);
            Assert.Equal("Bar", rawHeaders["Foo"]);
        }
    }
}
