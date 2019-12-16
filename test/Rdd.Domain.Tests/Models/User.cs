using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Rdd.Domain.Tests.Models
{
    public class User : IEntityBase<Guid>
    {
        private MailAddress _mail;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Mail
        {
            get => _mail?.ToString();
            set { _mail = string.IsNullOrEmpty(value) ? null : new MailAddress(value); }
        }

        public Uri TwitterUri { get; set; }
        public decimal Salary { get; set; }
        public Department Department { get; set; }
        public int? DepartmentId { get; set; }
        public Guid PictureId { get; set; }
        public Guid? FriendId { get; set; }

        public static IEnumerable<User> GetManyRandomUsers(int count) => GetManyRandomUsers(count, new List<Department>());
        public static IEnumerable<User> GetManyRandomUsers(int count, List<Department> departments)
        {
            var result = new List<User>();
            var random = new Random();
            for (var i = 1; i <= count; i++)
            {
                result.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Name = $"John Doe {i}",
                    FriendId = i % 2 == 0 ? (Guid?)null : Guid.NewGuid(),
                    DepartmentId = departments.Count == 0 ? (int?)null : departments[random.Next(0, departments.Count)].Id,
                    Mail = $"aaa{i}@example.com"
                });
            }

            return result;
        }

        public object GetId() => Id;
    }
}