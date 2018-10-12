using Rdd.Domain.Models;
using System;

namespace Rdd.Web.Tests.Models
{
    public class User : EntityBase<int>, IUser
    {
        public MyValueObject MyValueObject { get; set; }
        public Uri TwitterUri { get; set; }
        public decimal Salary { get; set; }
        public Department Department { get; set; }
        public Guid PictureId { get; set; }
        public DateTime? BirthDay { get; set; }
        public DateTime ContractStart { get; set; }
    }
}
