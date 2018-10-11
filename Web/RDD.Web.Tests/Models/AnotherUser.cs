using Rdd.Domain.Models;

namespace Rdd.Web.Tests.Models
{
    public class AnotherUser : EntityBase<int>, IUser
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
    }
}
