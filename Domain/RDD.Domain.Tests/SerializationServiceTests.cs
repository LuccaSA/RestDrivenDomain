using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using System.Collections.Generic;
using System.Net.Mail;
using Xunit;

namespace RDD.Domain.Tests
{
    public class SerializationServiceTests
    {
        [Fact]
        public void SerializeStringAsMailAddress_WHEN_GoodMailInQueryFilters()
        {
            var service = new StringConverter();
            var chain = new ExpressionParser().ParseChain(typeof(User), nameof(User.Mail));
            var values = service.ConvertValues(chain, new HashSet<string> { "mail@domain.com" });

            Assert.Equal(new MailAddress("mail@domain.com"), values[0]);
        }
    }
}
