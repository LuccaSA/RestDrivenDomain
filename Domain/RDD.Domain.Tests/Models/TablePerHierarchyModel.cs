using RDD.Domain.Models;

namespace RDD.Domain.Tests.Models
{
	public abstract class AbstractClass : EntityBase<AbstractClass, int>
	{
		public override int Id { get; set; }

		public override string Name { get; set; }
	}

	public class ConcreteClassOne : AbstractClass { }
	public class ConcreteClassTwo : AbstractClass { }

	public class ConcreteClassThree : EntityBase<ConcreteClassThree, int>
	{
		public override int Id { get; set; }
		public override string Name { get; set; }
	}
}
