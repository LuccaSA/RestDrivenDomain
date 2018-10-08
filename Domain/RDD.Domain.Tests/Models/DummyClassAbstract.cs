using System.Collections.Generic;

namespace Rdd.Domain.Tests.Models
{
	public abstract class DummyClassAbstract
	{
		public virtual string DummyProp { get; set; }
		public ICollection<DummySubClass> Children { get; set; }
		public DummySubClass BestChild { get; set; }
	}
}
