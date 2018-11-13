using Rdd.Domain.Exceptions;
using Rdd.Infra.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Xunit;

namespace Rdd.Infra.Tests.Exceptions
{
    public class ExceptionsTests
    {    
        public class ExceptionData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { new QueryBuilderException("lol") };
                yield return new object[] { new QueryBuilderException("lol", new Exception("")) };
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
