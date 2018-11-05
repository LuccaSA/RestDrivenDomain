using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Rdd.Domain.Tests.Models
{
    public class User : IEntityBase<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public string MailString { get; set; }
        public MailAddress Mail { get => new MailAddress(MailString); set => MailString = value?.ToString(); }
        public Uri TwitterUri { get; set; }
        public decimal Salary { get; set; }
        public Department Department { get; set; }
        public Guid PictureId { get; set; }
        public Guid? FriendId { get; set; }

        public static IEnumerable<User> GetManyRandomUsers(int count)
        {
            var result = new List<User>();

            for (var i = 1; i <= count; i++)
            {
                result.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Name = $"John Doe {i}",
                    FriendId = i % 2 == 0 ? (Guid?)null : Guid.NewGuid(),
                    MailString = $"aaa{i}@example.com"
                });
            }

            return result;
        }

        public object GetId() => Id;
    }
}