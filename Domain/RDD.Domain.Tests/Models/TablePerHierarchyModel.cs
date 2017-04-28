using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests.Models
{
	public abstract class AbstractClass : EntityBase<AbstractClass, int>
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
