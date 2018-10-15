using Rdd.Domain.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class ExceptionTests
    {
        public class ExceptionData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { new BadRequestException("lol") };
                yield return new object[] { new BadRequestException("lol", new Exception("")) };
                yield return new object[] { new ForbiddenException("lol") };
                yield return new object[] { new TechnicalException("lol") };
                yield return new object[] { new TechnicalException("lol", new Exception("")) };
                yield return new object[] { new UnauthorizedException("lol") };
                yield return new object[] { new UnsupportedContentTypeException("lol") };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(ExceptionData))]
        public void Exception<T>(T exception) where T : Exception, IStatusCodeException
        {
            var info = new SerializationInfo(exception.GetType(), new FormatterConverter());
            exception.GetObjectData(info, new StreamingContext(StreamingContextStates.All));

            Assert.Equal(exception.StatusCode, info.GetValue(nameof(BusinessException.StatusCode), typeof(HttpStatusCode)));
        }
    }
}