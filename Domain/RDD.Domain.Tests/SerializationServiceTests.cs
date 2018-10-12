using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests.Models;
using System.Collections.Generic;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class SerializationServiceTests
    {
        [Fact]
        public void SerializeStringAsMailAddress_WHEN_GoodMailInQueryFilters()
        {
            var service = new SerializationService();
            var values = service.ConvertWhereValues(new HashSet<string>() { "mail@domain.com" }, typeof(User).GetProperty("Mail"));
        }
    }
}
