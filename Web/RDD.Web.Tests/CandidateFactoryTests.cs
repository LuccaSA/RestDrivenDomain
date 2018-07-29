using Moq;
using RDD.Web.Helpers;
using RDD.Web.Tests.Models;
using Xunit;

namespace RDD.Web.Tests
{
    public class CandidateFactoryTests
    {
        [Fact]
        public void CandidateFactoryShouldDeserializeJson()
        {
            var json = @"{ ""id"": 123, ""name"": ""Foo"" }";
            var httpContextHelper = new Mock<IHttpContextHelper>();
            httpContextHelper.Setup(h => h.GetContent())
                .Returns(json);
            httpContextHelper.Setup(h => h.GetContentType())
                .Returns("application/json");

            var helper = new CandidateFactory<User, int>(httpContextHelper.Object);

            var candidate = helper.CreateCandidate();

            Assert.Equal(123, candidate.Value.Id);
            Assert.Equal("Foo", candidate.Value.Name);
        }
    }
}
