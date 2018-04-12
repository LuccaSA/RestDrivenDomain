using RDD.Domain;
using RDD.Domain.Models;
using System;

namespace RDD.Web.Tests.Models
{
    public class User : EntityBase<User, int>, IUser
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
        public MyValueObject MyValueObject { get; set; }
        public Uri TwitterUri { get; set; }
        public decimal Salary { get; set; }
        public Department Department { get; set; }
        public Guid PictureId { get; set; }

        IUser ICloneable<IUser>.Clone()
        {
            return this;
        }
    }
}
