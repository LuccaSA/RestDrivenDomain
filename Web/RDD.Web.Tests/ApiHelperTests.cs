using Rdd.Domain.Json;
using Rdd.Web.Querying;
using Rdd.Web.Tests.Models;
using Xunit;

namespace Rdd.Web.Tests
{
    public class ApiHelperTests
    {
        [Fact]
        public void ApiHelperShouldDeserializeJson()
        {
            var json = @"{ ""id"": 123, ""name"": ""Foo"" }";

            var candidate = new CandidateParser(new JsonParser()).Parse<User, int>(json);

            Assert.Equal(123, candidate.Value.Id);
            Assert.Equal("Foo", candidate.Value.Name);
        }
    }
}