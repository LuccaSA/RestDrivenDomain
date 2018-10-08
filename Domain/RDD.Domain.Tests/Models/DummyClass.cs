using System;
using System.Collections.Generic;

namespace Rdd.Domain.Tests.Models
{
	public class DummyClass
	{
		public string DummyProp { get; set; }
		public string DummyProp2 { get; set; }
		public ICollection<DummySubClass> Children { get; set; }
		public DummySubClass BestChild { get; set; }

		public DateTime Date { get; set; }

		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}
}
