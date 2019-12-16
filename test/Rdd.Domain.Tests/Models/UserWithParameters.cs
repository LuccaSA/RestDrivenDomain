using Rdd.Domain.Models;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Rdd.Domain.Tests.Models
{
    public class UserWithParameters : EntityBase<int>
    {
        private MailAddress _mail;

        public string Mail
        {
            get => _mail?.ToString();
            set { _mail = string.IsNullOrEmpty(value) ? null : new MailAddress(value); }
        }

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
