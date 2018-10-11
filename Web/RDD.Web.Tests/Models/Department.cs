using Rdd.Domain.Models;
using System.Collections.Generic;

namespace Rdd.Web.Tests.Models
{
    
    public enum Test { A = 0, B = 10 }
    public class Department : EntityBase<int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
        public ICollection<User> Users { get; set; }

        public Test? Enum { get; set; }

        public Department()
        {
            Users = new HashSet<User>();
        }
    }
}
