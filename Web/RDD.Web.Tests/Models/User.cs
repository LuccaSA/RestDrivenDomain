using RDD.Domain;
using RDD.Domain.Models;

namespace RDD.Web.Tests.Models
{
    public class User : EntityBase<User, int>, IUser
    {
        public override int Id { get; set; }
        public override string Name { get; set; }

        IUser ICloneable<IUser>.Clone()
        {
            return this;
        }
    }
}
