using Rdd.Domain.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class JsonTests
    {
        [Fact]
        public void Json()
        {
            var input = @"[{""key"": 1}, {""key"": 2}]";
            var json = new JsonParser().Parse(input);

            Assert.True(json.HasJsonArray((string)null));
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

        [Fact]
        public void JsonErrorCases()
        {
            var input = @"[{""key"": 1}, {""key"": 2}]";
            var json = new JsonParser().Parse(input);

            Assert.False(json.HasJsonArray("0"));
            Assert.False(json.HasJsonValue("0"));
            Assert.False(json.HasJsonObject(""));

            Assert.Throws<ArgumentException>(() => json.GetJsonArray("0"));
            Assert.Throws<ArgumentException>(() => json.GetJsonArray("key"));
            Assert.Throws<ArgumentException>(() => json.GetJsonValue("0"));
            Assert.Throws<ArgumentException>(() => json.GetJsonValue("0.path"));
            Assert.Throws<ArgumentException>(() => json.GetJsonObject(""));
            Assert.Throws<ArgumentException>(() => json.GetJsonObject("0.path"));
        }
    }
}