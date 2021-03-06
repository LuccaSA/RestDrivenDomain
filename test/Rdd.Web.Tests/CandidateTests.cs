using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rdd.Domain;
using Rdd.Domain.Json;
using Rdd.Domain.Tests.Models;
using Rdd.Web.Models;
using Rdd.Web.Querying;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Department = Rdd.Web.Tests.Models.Department;
using User = Rdd.Web.Tests.Models.User;

namespace Rdd.Web.Tests
{
    public class CandidateTests
    {
        private class OptionsAccessor : IOptions<MvcNewtonsoftJsonOptions>
        {
            public static MvcNewtonsoftJsonOptions JsonOptions = new MvcNewtonsoftJsonOptions();
            public MvcNewtonsoftJsonOptions Value => JsonOptions;
        }

        private readonly ICandidateParser _parser = new CandidateParser(new JsonParser(), new OptionsAccessor());

        private ICandidate<TEntity, TKey> Parse<TEntity, TKey>(string content)
            where TEntity : class, IPrimaryKey<TKey>
            => new CandidateParser(new JsonParser(), new OptionsAccessor()).Parse<TEntity, TKey>(content);

        public static Stream ToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void CandidateParserConstructor()
        {
            Assert.Throws<ArgumentNullException>("jsonParser", () => new CandidateParser(null, new OptionsAccessor()));
            Assert.Throws<ArgumentNullException>("jsonOptions", () => new CandidateParser(new JsonParser(), null));
        }

        [Fact]
        public void ApiHelperShouldDeserializeJson()
        {
            var json = @"{ ""id"": 123, ""name"": ""Foo"" }";

            var candidate = _parser.Parse<User, int>(json);

            Assert.Equal(123, candidate.Value.Id);
            Assert.Equal("Foo", candidate.Value.Name);
        }

        [Fact]
        public async Task ManyCandidates_should_exposeProperties_fromrequest()
        {
            var request = new DefaultHttpContext().Request;
            request.Body = ToStream(@"{ ""id"": 1, ""name"": ""User1"", ""salary"": 2000 }");
            var candidates = await _parser.ParseManyAsync<User, int>(request);

            Assert.Single(candidates);

            var candidate = candidates.First();

            Assert.True(candidate.HasProperty(u => u.Id));
            Assert.True(candidate.HasProperty(u => u.Name));
            Assert.True(candidate.HasProperty(u => u.Salary));

            Assert.False(candidate.HasProperty(u => u.TwitterUri));
        }

        [Fact]
        public void ManyCandidates_should_exposeProperties()
        {
            var candidates = _parser.ParseMany<User, int>(@"{ ""id"": 1, ""name"": ""User1"", ""salary"": 2000 }");

            Assert.Single(candidates);

            var candidate = candidates.First();

            Assert.True(candidate.HasProperty(u => u.Id));
            Assert.True(candidate.HasProperty(u => u.Name));
            Assert.True(candidate.HasProperty(u => u.Salary));

            Assert.False(candidate.HasProperty(u => u.TwitterUri));
        }

        [Fact]
        public async Task Candidate_should_exposeProperties_fromrequest()
        {
            var request = new DefaultHttpContext().Request;
            request.Body = ToStream(@"{ ""id"": 1, ""name"": ""User1"", ""salary"": 2000 }");
            var candidate = await _parser.ParseAsync<User, int>(request);

            Assert.True(candidate.HasProperty(u => u.Id));
            Assert.True(candidate.HasProperty(u => u.Name));
            Assert.True(candidate.HasProperty(u => u.Salary));

            Assert.False(candidate.HasProperty(u => u.TwitterUri));
        }

        [Fact]
        public void Candidate_should_exposeProperties()
        {
            var candidate = Parse<User, int>(@"{ ""id"": 1, ""name"": ""User1"", ""salary"": 2000 }");

            Assert.True(candidate.HasProperty(u => u.Id));
            Assert.True(candidate.HasProperty(u => u.Name));
            Assert.True(candidate.HasProperty(u => u.Salary));

            Assert.False(candidate.HasProperty(u => u.TwitterUri));
        }

        [Fact]
        public void Candidate_should_exposeNeastedProperties()
        {
            var candidate = Parse<User, int>(@"{ ""id"": 1, ""name"": ""User1"", ""department"": { ""id"": 2 } }");

            Assert.True(candidate.HasProperty(u => u.Id));
            Assert.True(candidate.HasProperty(u => u.Name));
            Assert.True(candidate.HasProperty(u => u.Department));
            Assert.True(candidate.HasProperty(u => u.Department.Id));

            Assert.False(candidate.HasProperty(u => u.Department.Name));
            Assert.False(candidate.HasProperty(u => u.TwitterUri));
        }

        [Fact]
        public void Candidate_should_exposeMultipleNeastedProperties()
        {
            var candidate = Parse<User, int>(@"{ ""id"": 1, ""department"": { ""id"": 2, ""name"": ""Dep2"" } }");

            Assert.True(candidate.HasProperty(u => u.Id));
            Assert.True(candidate.HasProperty(u => u.Department));
            Assert.True(candidate.HasProperty(u => u.Department.Id));
            Assert.True(candidate.HasProperty(u => u.Department.Name));

            Assert.False(candidate.HasProperty(u => u.TwitterUri));
        }

        [Fact]
        public void Candidate_should_exposeArrayProperties()
        {
            var candidate = Parse<Department, int>(@"{ ""id"": 1, ""users"": [ { ""id"": 2 }, { ""id"": 3 } ] }");

            Assert.True(candidate.HasProperty(d => d.Id));
            Assert.True(candidate.HasProperty(d => d.Users));
            Assert.True(candidate.HasProperty(d => d.Users.Select(u => u.Id)));

            Assert.False(candidate.HasProperty(d => d.Name));
            Assert.False(candidate.HasProperty(d => d.Users.Select(u => u.Name)));
        }

        [Fact]
        public void Candidate_should_fail_without_config()
        {
            OptionsAccessor.JsonOptions.SerializerSettings.Converters = new List<JsonConverter>();

            Assert.Throws<JsonSerializationException>(GetCandidate);
        }

        [Fact]
        public void Candidate_should_work_with_config()
        {
            OptionsAccessor.JsonOptions.SerializerSettings.Converters = new List<JsonConverter>
            {
                new BaseClassJsonConverter<Hierarchy>(new InheritanceConfiguration())
            };

            var candidate = GetCandidate();
            Assert.True(candidate.HasProperty(d => d.Id));
        }

        private static ICandidate<Hierarchy, int> GetCandidate()
            => new CandidateParser(new JsonParser(), new OptionsAccessor()).Parse<Hierarchy, int>(@"{ ""id"": 1, ""type"":""super"", ""superProperty"": ""lol"" }");
    }
}