using Newtonsoft.Json;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RDD.Domain.Tests.Models.Querying
{
    public class CandidateTests
    {
        [Fact]
        public void Candidate_should_exposeProperties()
        {
            var candidate = new Candidate<User>(@"{ ""id"": 1, ""name"": ""User1"", ""salary"": 2000 }");

            Assert.True(candidate.HasValue(u => u.Id));
            Assert.True(candidate.HasValue(u => u.Name));
            Assert.True(candidate.HasValue(u => u.Salary));

            Assert.False(candidate.HasValue(u => u.TwitterUri));
        }

        [Fact]
        public void Candidate_should_exposeNeastedProperties()
        {
            var candidate = new Candidate<User>(@"{ ""id"": 1, ""name"": ""User1"", ""department"": { ""id"": 2 } }");

            Assert.True(candidate.HasValue(u => u.Id));
            Assert.True(candidate.HasValue(u => u.Name));
            Assert.True(candidate.HasValue(u => u.Department));
            Assert.True(candidate.HasValue(u => u.Department.Id));

            Assert.False(candidate.HasValue(u => u.Department.Name));
            Assert.False(candidate.HasValue(u => u.TwitterUri));
        }

        [Fact]
        public void Candidate_should_exposeMultipleNeastedProperties()
        {
            var candidate = new Candidate<User>(@"{ ""id"": 1, ""department"": { ""id"": 2, ""name"": ""Dep2"" } }");

            Assert.True(candidate.HasValue(u => u.Id));
            Assert.True(candidate.HasValue(u => u.Department));
            Assert.True(candidate.HasValue(u => u.Department.Id));
            Assert.True(candidate.HasValue(u => u.Department.Name));

            Assert.False(candidate.HasValue(u => u.TwitterUri));
        }

        [Fact]
        public void Candidate_should_exposeArrayProperties()
        {
            var candidate = new Candidate<Department>(@"{ ""id"": 1, ""users"": [ { ""id"": 2 }, { ""id"": 3 } ] }");

            Assert.True(candidate.HasValue(d => d.Id));
            Assert.True(candidate.HasValue(d => d.Users));
            Assert.True(candidate.HasValue(d => d.Users.Select(u => u.Id)));

            Assert.False(candidate.HasValue(d => d.Name));
            Assert.False(candidate.HasValue(d => d.Users.Select(u => u.Name)));
        }
    }
}
