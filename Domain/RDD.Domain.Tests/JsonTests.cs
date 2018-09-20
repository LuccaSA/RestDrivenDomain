using RDD.Domain.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace RDD.Domain.Tests
{
    public class JsonTests
    {
        [Fact]
        public void Json()
        {
            var input = @"[{""key"": 1}, {""key"": 2}]";
            var json = new JsonParser().Parse(input);

            Assert.True(json.HasJsonArray(""));
            Assert.True(json.HasJsonObject("0"));
            Assert.True(json.HasJsonValue("0.key"));
            Assert.Equal(new HashSet<string> { "0.key", "1.key" }, json.GetPaths());

            var array = json.GetJsonArray("");
            Assert.Equal(2, (json.GetContent() as Array).Length);
            Assert.Equal(new List<string> { "1", "2" }, array.GetEveryJsonValue("key"));

            var jObject = json.GetJsonObject("0");
            Assert.Single(jObject.GetContent() as Dictionary<string, object>);

            Assert.Equal("1", json.GetJsonValue("0.key"));
        }
    }
}