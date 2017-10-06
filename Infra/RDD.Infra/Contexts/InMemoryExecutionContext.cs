using RDD.Domain;
using System.Diagnostics;

namespace RDD.Infra.Contexts
{
	public class InMemoryExecutionContext : IExecutionContext
	{
		public IPrincipal curPrincipal { get; set; }
		public Stopwatch queryWatch { get; private set; }

		public InMemoryExecutionContext()
		{
			queryWatch = new Stopwatch();
		}
	}
}
