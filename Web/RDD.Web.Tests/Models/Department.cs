using RDD.Domain.Models;
using System.Collections.Generic;

namespace RDD.Web.Tests.Models
{
    public class Department : EntityBase<Department, int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
        public ICollection<User> Users { get; set; }

        public Department()
        {
            Users = new HashSet<User>();
        }
    }
}
