using Rdd.Domain.Models;
using System.Collections.Generic;

namespace Rdd.Web.Tests.Models
{
    public class Department : EntityBase<int>
    {
        public ICollection<User> Users { get; set; }

        public Department()
        {
            Users = new HashSet<User>();
        }
    }
}
