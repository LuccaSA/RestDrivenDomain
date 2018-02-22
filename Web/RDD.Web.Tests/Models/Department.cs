using System.Collections.Generic;

namespace RDD.Web.Tests.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<User> Users { get; set; }

        public Department()
        {
            Users = new HashSet<User>();
        }
    }
}
