using Rdd.Domain.Helpers.Expressions.Equality;
using Rdd.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Rdd.Domain.Tests.Members
{
    public class ExpressionChainExtractorTests
    {
        private class User
        {
            public int Id { get; set; }
            public User Manager { get; set; }
            public List<User> Collaborators { get; set; }
            public List<Role> HabilitedRoles { get; set; }
        }

        private class Department
        {
            public User Head { get; set; }

            public Dictionary<int, User> EmployeeOfTheMonth { get; set; }
        }

        private class Role
        {
            public int Id { get; set; }
            public bool HasContextualLegalEntityAssociation { get; set; }
        }

        [Fact]
        public void ExpressionToChain1()
        {
            Expression<Func<Department, User>> lambda = u => u.Head;
            var chain = ExpressionChainExtractor.AsExpressionChain(lambda);

            Assert.Equal(nameof(Department.Head), chain.Current.Name);

            Assert.True(new ExpressionEqualityComparer().Equals(lambda, chain.ToLambdaExpression()));
        }

        [Fact]
        public void ExpressionToChain2()
        {
            Expression<Func<Department, IEnumerable<int>>> lambda = u => u.Head.Collaborators.Select(c => c.Id);
            var chain = ExpressionChainExtractor.AsExpressionChain(lambda);

            Assert.Equal(nameof(Department.Head), chain.Current.Name);
            Assert.Equal(nameof(User.Collaborators), chain.Next.Current.Name);
            Assert.Equal(nameof(User.Id), chain.Next.Next.Current.Name);

            Assert.True(new ExpressionEqualityComparer().Equals(lambda, chain.ToLambdaExpression()));
        }
    }
}