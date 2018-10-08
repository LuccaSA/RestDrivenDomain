using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests.Models;
using Xunit;

namespace Rdd.Domain.Tests
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
