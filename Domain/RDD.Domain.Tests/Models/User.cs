using Rdd.Domain.Models;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Rdd.Domain.Tests.Models
{
    public class User : EntityBase<User, Guid>
    {
        public override Guid Id { get; set; }
        public override string Name { get; set; }
        public MailAddress Mail { get; set; }
        public Uri TwitterUri { get; set; }
        public decimal Salary { get; set; }
        public Department Department { get; set; }
        public Guid PictureId { get; set; }

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
