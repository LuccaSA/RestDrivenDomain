using Newtonsoft.Json;
using RDD.Domain.Mocks;
using RDD.Web.Models;
using RDD.Web.Tests.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RDD.Web.Tests
{
    public class CandidateTests
    {
        [Fact]
        public void Candidate_should_exposeProperties()
        {
            var candidate = Candidate<User, int>.Parse(@"{ ""id"": 1, ""name"": ""User1"", ""salary"": 2000 }");

            Assert.True(candidate.HasProperty(u => u.Id));
            Assert.True(candidate.HasProperty(u => u.Name));
            Assert.True(candidate.HasProperty(u => u.Salary));

            Assert.False(candidate.HasProperty(u => u.TwitterUri));
        }

        [Fact]
        public void Candidate_should_exposeNeastedProperties()
        {
            var candidate = Candidate<User, int>.Parse(@"{ ""id"": 1, ""name"": ""User1"", ""department"": { ""id"": 2 } }");

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
            var candidate = Candidate<User, int>.Parse(@"{ ""id"": 1, ""department"": { ""id"": 2, ""name"": ""Dep2"" } }");

            Assert.True(candidate.HasProperty(u => u.Id));
            Assert.True(candidate.HasProperty(u => u.Department));
            Assert.True(candidate.HasProperty(u => u.Department.Id));
            Assert.True(candidate.HasProperty(u => u.Department.Name));

            Assert.False(candidate.HasProperty(u => u.TwitterUri));
        }

        [Fact]
        public void Candidate_should_exposeArrayProperties()
        {
            var candidate = Candidate<Department, int>.Parse(@"{ ""id"": 1, ""users"": [ { ""id"": 2 }, { ""id"": 3 } ] }");

            Assert.True(candidate.HasProperty(d => d.Id));
            Assert.True(candidate.HasProperty(d => d.Users));
            Assert.True(candidate.HasProperty(d => d.Users.Select(u => u.Id)));

            Assert.False(candidate.HasProperty(d => d.Name));
            Assert.False(candidate.HasProperty(d => d.Users.Select(u => u.Name)));
        }

        [Fact]
        public void Candidate_should_fail_without_config()
        {
            JsonConvert.DefaultSettings = null;
            Assert.Throws<JsonSerializationException>(GetCandidate);
        }

        [Fact]
        public void Candidate_should_work_with_config()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new BaseClassJsonConverter<Hierarchy>(new InheritanceConfiguration())
                }
            };

            var candidate = GetCandidate();
            Assert.True(candidate.HasProperty(d => d.Id));
        }

        private static Candidate<Hierarchy, int> GetCandidate() => Candidate<Hierarchy, int>.Parse(@"{ ""id"": 1, ""type"":""super"", ""superProperty"": ""lol"" }");
    }
}
