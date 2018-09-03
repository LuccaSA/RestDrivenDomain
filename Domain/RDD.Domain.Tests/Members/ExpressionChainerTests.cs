using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace RDD.Domain.Tests.Members
{
    public class ExpressionChainerTests
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
        public void Expressions2Chainer()
        {
            Expression<Func<User, int>> lambda1 = u => u.Id;
            Expression<Func<int, bool>> lambda2 = i => i == 2;
            var result = ExpressionChainer.Chain(lambda1, lambda2);
            Expression<Func<User, bool>> expected = a => a.Id == 2;

            Assert.True(ExpressionEqualityComparer.Eq(expected, result));
        }

        [Fact]
        public void Expressions3Chainer()
        {
            Expression<Func<Department, User>> lambda1 = u => u.Head;
            Expression<Func<User, bool>> lambda2 = i => i.Id == 2;
            var result = ExpressionChainer.Chain(lambda1, lambda2);
            Expression<Func<Department, bool>> expected = a => a.Head.Id == 2;

            Assert.True(ExpressionEqualityComparer.Eq(expected, result));
        }

        [Fact]
        public void Expressions4Chainer()
        {
            Expression<Func<User, User>> lambda1 = u => u.Manager;
            Expression<Func<User, bool>> lambda2 = m => m.HabilitedRoles.Any(r => r.HasContextualLegalEntityAssociation);
            var result = ExpressionChainer.Chain(lambda1, lambda2);
            Expression<Func<User, bool>> expected = a => a.Manager.HabilitedRoles.Any(r => r.HasContextualLegalEntityAssociation);

            Assert.True(ExpressionEqualityComparer.Eq(expected, result));
        }

        [Fact]
        public void Expressions5Chainer()
        {
            Expression<Func<User, List<Role>>> lambda1 = u => u.HabilitedRoles;
            Expression<Func<Role, int>> lambda2 = m => m.Id;
            var result = ExpressionChainer.Chain(lambda1, lambda2);
            Expression<Func<User, IEnumerable<int>>> expected = u => u.HabilitedRoles.Select(m => m.Id);

            Assert.True(ExpressionEqualityComparer.Eq(expected, result));
        }

        [Fact]
        public void Expressions6Chainer()
        {
            Expression<Func<User, List<User>>> lambda1 = u => u.Collaborators;
            Expression<Func<User, User>> lambda2 = m => m.Manager;
            var result = ExpressionChainer.Chain(lambda1, lambda2);
            Expression<Func<User, IEnumerable<User>>> expected = u => u.Collaborators.Select(m => m.Manager);

            Assert.True(ExpressionEqualityComparer.Eq(expected, result));
        }

        [Fact]
        public void Expressions7Chainer()
        {
            Expression<Func<User, User>> lambda1 = m => m.Manager;
            Expression<Func<User, IEnumerable<User>>> lambda2 = u => u.Collaborators.Select(c => c);
            var result = ExpressionChainer.Chain(lambda1, lambda2);
            Expression<Func<User, IEnumerable<User>>> expected = u => u.Manager.Collaborators.Select(c => c);

            Assert.True(ExpressionEqualityComparer.Eq(expected, result));
        }

        [Fact]
        public void Expressions8Chainer()
        {
            Expression<Func<Department, User>> lambda1 = d => d.EmployeeOfTheMonth[1];
            Expression<Func<User, IEnumerable<User>>> lambda2 = u => u.Collaborators.Select(c => c);
            var result = ExpressionChainer.Chain(lambda1, lambda2);
            Expression<Func<Department, IEnumerable<User>>> expected = u => u.EmployeeOfTheMonth[1].Collaborators.Select(c => c);

            Assert.True(ExpressionEqualityComparer.Eq(expected, result));
        }
    }
}