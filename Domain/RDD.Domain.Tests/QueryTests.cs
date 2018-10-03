using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using System;
using Xunit;

namespace RDD.Domain.Tests
{
    public class QueryTests
    {
        [Fact]
        public void Cloning_query_should_clone_verb()
        {
            var query = new Query<User> { Verb = HttpVerbs.Put };
            var result = new Query<User>(query);

            Assert.Equal(HttpVerbs.Put, result.Verb);
        }

        [Fact]
        public void Cloning_query_should_not_clone_stopwatch()
        {
            var query = new Query<User> { Verb = HttpVerbs.Put };
            var result = new Query<User>(query);

            Assert.NotEqual(query.Watch, result.Watch);
        }
    }
}
