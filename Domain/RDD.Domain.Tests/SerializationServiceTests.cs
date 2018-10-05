using RDD.Domain.Models.Querying;
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
            var values = service.ConvertValues<MailAddress>(new HashSet<string> { "mail@domain.com" });

            Assert.Equal(new MailAddress("mail@domain.com"), values[0]);
        }
    }
}
