using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using System.Collections.Generic;
using Xunit;

namespace RDD.Domain.Tests
{
    public class SerializationServiceTests
    {
        [Fact]
        public void SerializeStringAsMailAddress_WHEN_GoodMailInQueryFilters()
        {
            var service = new DeserializationService();
            var values = service.ConvertWhereValues(new HashSet<string>() { "mail@domain.com" }, typeof(User).GetProperty("Mail"));
        }
    }
}
