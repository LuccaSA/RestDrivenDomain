using RDD.Domain.Models;
using System;

namespace RDD.Domain.Tests.Models
{
	public abstract class AbstractClass : EntityBase<int>
	{
		public override int Id
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override string Name
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public class ConcreteClassOne : AbstractClass { }
	public class ConcreteClassTwo : AbstractClass { }
}
