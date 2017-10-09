using System.Collections.Generic;

namespace RDD.Domain.Models
{
	public class Application : EntityBase<Application, string>
	{
		public override string Id { get; set; }
		public override string Name { get; set; }
		public ICollection<Combination> Combinations { get; set; }
		public new Dictionary<int, Operation> Operations { get; set; }
	}
}
