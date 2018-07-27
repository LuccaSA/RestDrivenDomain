using Microsoft.AspNetCore.Http;
using Moq;
using RDD.Web.Helpers;
using RDD.Web.Tests.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RDD.Web.Tests
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

            var helper = new ApiHelper<User, int>(httpContextHelper.Object, null);

            var candidate = helper.CreateCandidate();

            Assert.Equal(123, candidate.Value.Id);
            Assert.Equal("Foo", candidate.Value.Name);
        }
    }
}
