using RDD.Domain.Helpers;
using RDD.Domain.Tests.Models;
using Xunit;

namespace RDD.Domain.Tests
{
    public class PropertySelectorTests
    {
        [Fact]
        public void Parsing_count_on_empty_collection()
        {
            var field = "count";
            var selector = new CollectionPropertySelector<User>();
            selector.Parse(field);
        }

        [Fact]
        public void NeastedSelector_should_work()
        {
            var selector = new PropertySelector<User>(u => u.TwitterUri.Host);

            Assert.True(selector.Contains(u => u.TwitterUri));
            Assert.True(selector.Contains(u => u.TwitterUri.Host));
        }
    }
}
