using Newtonsoft.Json.Linq;
using NExtends.Primitives.DateTimes;
using Rdd.Domain.Helpers;
using Rdd.Domain.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class JsonTests
    {
        [Fact]
        public void JsonValueTests()
        {
            var value = new JsonValue("poney");

            Assert.True(value.HasKey((string)null));
            Assert.True(value.HasKey(""));
            Assert.True(value.HasKey((Queue<string>)null));

            Assert.True(value.HasJsonValue(""));
            Assert.False(value.HasJsonArray(""));
            Assert.False(value.HasJsonObject(""));
            Assert.False(value.HasJsonValue("sss"));

            Assert.Equal("poney", value.GetJsonValue(""));
            Assert.Throws<ArgumentException>(() => value.GetJsonArray(""));
            Assert.Throws<ArgumentException>(() => value.GetJsonObject(""));
            Assert.Throws<ArgumentException>(() => value.GetJsonValue("sss"));
        }

        [Fact]
        public void JsonObjectTests()
        {
            var value = new JsonObject(new Dictionary<string, string> { { "aaa", "bbb" } });

            Assert.True(value.HasKey((string)null));
            Assert.True(value.HasKey(""));
            Assert.True(value.HasKey((Queue<string>)null));
            Assert.False(value.HasKey("nope"));

            Assert.False(value.HasJsonValue(""));
            Assert.Throws<ArgumentException>(() => value.GetJsonValue(""));
            Assert.True(value.HasJsonValue("aaa"));
            Assert.Equal("bbb", value.GetJsonValue("aaa"));
            Assert.False(value.HasJsonValue("nope"));
            Assert.Throws<ArgumentException>(() => value.GetJsonValue("nope"));

            Assert.False(value.HasJsonArray(""));
            Assert.Throws<ArgumentException>(() => value.GetJsonArray(""));
            Assert.False(value.HasJsonArray("nope"));
            Assert.Throws<ArgumentException>(() => value.GetJsonArray("nope"));

            Assert.True(value.HasJsonObject(""));
            Assert.Equal(value, value.GetJsonObject(""));
            Assert.False(value.HasJsonObject("nope"));
            Assert.Throws<ArgumentException>(() => value.GetJsonObject("nope"));
        }

        [Fact]
        public void JsonArrayTests()
        {
            var jobj = new JsonObject("aaa", new JsonValue("bbb"));
            var value = new JsonArray(jobj.Yield());

            Assert.True(value.HasKey((string)null));
            Assert.True(value.HasKey(""));
            Assert.True(value.HasKey((Queue<string>)null));
            Assert.True(value.HasKey("0"));
            Assert.False(value.HasKey("nope"));

            Assert.False(value.HasJsonValue(""));
            Assert.Throws<ArgumentException>(() => value.GetJsonValue(""));
            Assert.False(value.HasJsonValue("nope"));
            Assert.Throws<ArgumentException>(() => value.GetJsonValue("nope"));
            Assert.False(value.HasJsonValue("-23"));
            Assert.Throws<ArgumentException>(() => value.GetJsonValue("-23"));
            Assert.False(value.HasJsonValue("23"));
            Assert.Throws<ArgumentException>(() => value.GetJsonValue("23"));
            Assert.False(value.HasJsonValue("0"));
            Assert.Throws<ArgumentException>(() => value.GetJsonValue("0"));

            Assert.True(value.HasJsonArray(""));
            Assert.Equal(value, value.GetJsonArray(""));
            Assert.False(value.HasJsonArray("nope"));
            Assert.Throws<ArgumentException>(() => value.GetJsonArray("nope"));
            Assert.False(value.HasJsonArray("-23"));
            Assert.Throws<ArgumentException>(() => value.GetJsonArray("-23"));
            Assert.False(value.HasJsonArray("23"));
            Assert.Throws<ArgumentException>(() => value.GetJsonArray("23"));
            Assert.False(value.HasJsonArray("0"));
            Assert.Throws<ArgumentException>(() => value.GetJsonArray("0"));

            Assert.False(value.HasJsonObject(""));
            Assert.Throws<ArgumentException>(() => value.GetJsonObject(""));
            Assert.False(value.HasJsonObject("nope"));
            Assert.Throws<ArgumentException>(() => value.GetJsonObject("nope"));
            Assert.False(value.HasJsonObject("-23"));
            Assert.Throws<ArgumentException>(() => value.GetJsonObject("-23"));
            Assert.False(value.HasJsonObject("23"));
            Assert.Throws<ArgumentException>(() => value.GetJsonObject("23"));
            Assert.True(value.HasJsonObject("0"));
            Assert.Equal(jobj, value.GetJsonObject("0"));
            Assert.Throws<ArgumentException>(() => value.GetJsonValue("0"));
        }

        [Fact]
        public void Json()
        {
            var anonymous = new { Date = DateTime.Today };
            var json = new JsonParser().ParseFromAnonymous(anonymous);
            Assert.Equal(DateTime.Today.ToISOz(), json.GetJsonValue("Date"));

            Assert.Null(new JsonParser().Parse((JToken)null));
            var input = @"[{""key"": 1}, {""key"": 2}]";
            json = new JsonParser().Parse(input);

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