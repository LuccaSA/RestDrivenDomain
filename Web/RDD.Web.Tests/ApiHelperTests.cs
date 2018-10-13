using Microsoft.Extensions.Options;
using Moq;
using Rdd.Web.Helpers;
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
            var httpContextHelper = new Mock<IHttpContextHelper>();
            httpContextHelper.Setup(h => h.GetContent())
                .Returns(json);
            httpContextHelper.Setup(h => h.GetContentType())
                .Returns("application/json");

            var options = Options.Create(new RddOptions());
            var queryBuilder = new WebQueryFactory<User, int>(options);

            var helper = new ApiHelper<User, int>(httpContextHelper.Object, queryBuilder);

            var candidate = helper.CreateCandidate();

            Assert.Equal(123, candidate.Value.Id);
            Assert.Equal("Foo", candidate.Value.Name);
        }
    }
}
