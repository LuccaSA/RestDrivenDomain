using RDD.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Contexts
{
	public class InMemoryExecutionContext : IExecutionContext
	{
		public IPrincipal curPrincipal { get; set; }
		public Stopwatch serverWatch { get; private set; }
		public Stopwatch queryWatch { get; private set; }

		public InMemoryExecutionContext()
		{
			serverWatch = new Stopwatch();
			queryWatch = new Stopwatch();

			serverWatch.Start();
		}
	}
}
