using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Tests.Models;
using Xunit;

namespace RDD.Domain.Tests
{
    public class PropertySelectorTests
    {
        [Fact]
        public void Parsing_count_on_empty_collection()
        {
            var tree = new ExpressionSelectorParser().Parse(typeof(ISelection<User>), "count");

            Assert.Equal(nameof(ISelection.Count), tree.ToString());
        }
 
        [Fact]
        public void SimpleSelector_should_work()
        {
            var selector = ExpressionSelectorChain<User>.New(u => u.TwitterUri);

            Assert.True(selector.Contains(u => u.TwitterUri));

            Assert.Equal("TwitterUri", selector.Name);
        }

        [Fact]
        public void NeastedSelector_should_work()
        {
            var selector = ExpressionSelectorChain<User>.New(u => u.TwitterUri.Host);

            Assert.True(selector.Contains(u => u.TwitterUri));
            Assert.True(selector.Contains(u => u.TwitterUri.Host));

            Assert.Equal("TwitterUri.Host", selector.Name);
        }
    }
}
