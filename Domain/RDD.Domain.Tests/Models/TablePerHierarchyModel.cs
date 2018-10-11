using Rdd.Domain.Models;

namespace Rdd.Domain.Tests.Models
{
    public abstract class AbstractClass : EntityBase<int>
    {
        public override int Id { get; set; }

        public override string Name { get; set; }
    }

    public class ConcreteClassOne : AbstractClass { }
    public class ConcreteClassTwo : AbstractClass { }

    public class ConcreteClassThree : EntityBase<int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
    }
}
