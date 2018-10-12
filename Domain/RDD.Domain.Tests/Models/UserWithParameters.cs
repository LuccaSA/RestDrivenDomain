using Rdd.Domain.Models;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Rdd.Domain.Tests.Models
{
    public class UserWithParameters : EntityBase<UserWithParameters, int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
        public MailAddress Mail { get; set; }
        public Uri TwitterUri { get; set; }
        public decimal Salary { get; set; }

        public UserWithParameters(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public static IEnumerable<User> GetManyRandomUsers(int howMuch)
        {
            var result = new List<User>();

            for (var i = 1; i <= howMuch; i++)
            {
                var name = $"John Doe {i}";
                var id = Guid.NewGuid();

                result.Add(new User { Id = id, Name = name });
            }

            return result;
        }
    }
}
