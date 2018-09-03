using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace RDD.Domain.Tests.Members
{
    public class ExpressionChainExtractorTests
    {
        class User
        {
            public int Id { get; set; }
            public User Manager { get; set; }
            public List<User> Collaborators { get; set; }
            public List<Role> HabilitedRoles { get; set; }
        }
        class Department
        {
            public User Head { get; set; }

            public Dictionary<int, User> EmployeeOfTheMonth { get; set; }
        }
        class Role
        {
            public int Id { get; set; }
            public bool HasContextualLegalEntityAssociation { get; set; }
        }

        [Fact]
        public void ExpressionToChain1()
        {
            Expression<Func<Department, User>> lambda = u => u.Head;
            var chain = ExpressionChainExtractor.AsExpressionSelectorChain(lambda);

            Assert.Equal(nameof(Department.Head), chain.Current.Name);

            Assert.True(ExpressionEqualityComparer.Eq(lambda, chain.ToLambdaExpression()));
        }

        [Fact]
        public void ExpressionToChain2()
        {
            Expression<Func<Department, IEnumerable<int>>> lambda = u => u.Head.Collaborators.Select(c => c.Id);
            var chain = ExpressionChainExtractor.AsExpressionSelectorChain(lambda);

            Assert.Equal(nameof(Department.Head), chain.Current.Name);
            Assert.Equal(nameof(User.Collaborators), chain.Next.Current.Name);
            Assert.Equal(nameof(User.Id), chain.Next.Next.Current.Name);

            Assert.True(ExpressionEqualityComparer.Eq(lambda, chain.ToLambdaExpression()));
        }
    }
}