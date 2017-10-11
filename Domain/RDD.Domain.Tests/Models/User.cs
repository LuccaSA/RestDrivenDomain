using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace RDD.Domain.Tests.Models
{
    public class User : EntityBase<User, int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
        public MailAddress Mail { get; set; }
        public Uri TwitterUri { get; set; }
        public decimal Salary { get; set; }

        public static IEnumerable<User> GetManyRandomUsers(int howMuch)
        {
            var result = new List<User>();

            for (var i = 1; i <= howMuch; i++)
            {
                var name = $"John Doe {i}";

                result.Add(new User { Id = i, Name = name });
            }

            return result;
        }
    }
}
