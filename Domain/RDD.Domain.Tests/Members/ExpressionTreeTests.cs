using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace RDD.Domain.Tests.Members
{
    public class ExpressionTreeTests
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
        public void Expressions1Intersection()
        {
            var tree1 = ExpressionSelectorTree<User>.New(u => u.Collaborators.Select(c => c.HabilitedRoles), (User u) => u.Manager.Manager.Manager);
            var tree2 = ExpressionSelectorTree<User>.New(u => u.HabilitedRoles, (User u) => u.Manager.Manager.HabilitedRoles);

            var intersection = tree1.Intersection(tree2);
            var result = ExpressionSelectorTree<User>.New(u => u.Manager.Manager);
            Assert.Equal(result, intersection);
        }

        [Fact]
        public void Expressions2Intersection()
        {
            var tree1 = ExpressionSelectorTree<User>.New(u => u.Collaborators.Select(c => c.Manager), (User u) => u.Collaborators.Select(c => c.HabilitedRoles.Select(v => v.Id)));
            var tree2 = ExpressionSelectorTree<User>.New(u => u.Collaborators.Select(c => c.HabilitedRoles));

            var intersection = tree1.Intersection(tree2);
            Assert.Equal(tree2, intersection);
        }

        [Fact]
        public void ExpressionsTreeName()
        {
            var tree1 = ExpressionSelectorTree<User>.New(u => u.Collaborators.Select(c => c.Manager), (User u) => u.Collaborators.Select(c => c.HabilitedRoles.Select(v => v.Id)));

            var names = new HashSet<string>(tree1.Select(t => t.Name));
            var result = new HashSet<string> { "Collaborators.Manager", "Collaborators.HabilitedRoles.Id" };
            Assert.Equal(result, names);

            Assert.Equal("Collaborators[Manager,HabilitedRoles.Id]", tree1.ToString());
        }
    }
}