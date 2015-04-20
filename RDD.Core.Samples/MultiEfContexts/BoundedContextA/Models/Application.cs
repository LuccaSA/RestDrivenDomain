using RDD.Infra;
using RDD.Infra.Models.Entities;
using RDD.Infra.Models.Rights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Core.Samples.MultiEfContexts.BoundedContextA.Models
{
	public class Application : EntityBase<Application, string>, IApplication
	{
		public override string Id { get; set; }
		public override string Name { get; set; }
		public new ICollection<Operation> Operations { get; private set; }
		public ICollection<Combination> Combinations { get; private set; }

		public Application()
		{
			Operations = new HashSet<Operation>();
			Combinations = new HashSet<Combination>();
		}
	}
}
