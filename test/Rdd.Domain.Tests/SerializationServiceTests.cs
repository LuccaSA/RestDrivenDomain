using Rdd.Domain.Models.Querying;
using System.Collections.Generic;
using System.Net.Mail;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class SerializationServiceTests
    {
        [Fact]
        public void SerializeStringAsMailAddress_WHEN_GoodMailInQueryFilters()
        {
            var service = new StringConverter();
            var values = service.ConvertValues<MailAddress>("mail@domain.com");
            var converted = values as FilterValue<MailAddress>;

            Assert.Equal(new MailAddress("mail@domain.com"), converted.Value);
        }
    }
}
