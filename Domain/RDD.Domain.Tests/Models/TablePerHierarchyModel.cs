using Rdd.Domain.Models;

namespace Rdd.Domain.Tests.Models
{
    public abstract class AbstractClass : EntityBase<int> { }

    public class ConcreteClassOne : AbstractClass { }
    public class ConcreteClassTwo : AbstractClass { }

    public class ConcreteClassThree : EntityBase<int> { }
}
