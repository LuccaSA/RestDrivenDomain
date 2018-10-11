using Moq;
using Rdd.Web.Helpers;
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
            var httpContextHelper = new Mock<IHttpContextHelper>();
            httpContextHelper.Setup(h => h.GetContent())
                .Returns(json);
            httpContextHelper.Setup(h => h.GetContentType())
                .Returns("application/json");

            var helper = new ApiHelper<User, int>(httpContextHelper.Object);

            var candidate = helper.CreateCandidate();

            Assert.Equal(123, candidate.Value.Id);
            Assert.Equal("Foo", candidate.Value.Name);
        }
    }
}
