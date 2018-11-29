using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using Xunit;

namespace Rdd.Infra.Tests.Storage
{
    public class PropertyAuthorizerTests
    {
        [Theory]
        [InlineData(null, null, true)]
        [InlineData(null, "name", true)]
        [InlineData(null, "department", true)]
        [InlineData(null, "department.name", false)]
        [InlineData("department", "department.name", true)]
        [InlineData(null, "department.users", false)]
        [InlineData("department", "department.users", true)]
        [InlineData("department", "department.users.id", false)]
        [InlineData("department.users", "department.users.id", true)]
        [InlineData("department.users.Department", "department.users.id", true)]
        public void PropertyAuthorizer(string whiteList, string input, bool result)
        {
            var parser = new ExpressionParser();
            var authorizer = whiteList == null ? new PropertyAuthorizer<User>() : new PropertyAuthorizer<User>(parser.ParseTree<User>(whiteList));
            Assert.Equal(result, authorizer.IsVisible(input == null ? null : parser.ParseChain<User>(input)));
        }
    }
}