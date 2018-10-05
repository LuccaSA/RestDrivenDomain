using RDD.Domain.Json;
using RDD.Web.Querying;
using RDD.Web.Tests.Models;
using Xunit;

namespace RDD.Web.Tests
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