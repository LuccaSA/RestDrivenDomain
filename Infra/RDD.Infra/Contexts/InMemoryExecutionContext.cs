using RDD.Domain;
using System.Diagnostics;

namespace RDD.Infra.Contexts
{
	public class InMemoryExecutionContext : IExecutionContext
	{
		public IPrincipal curPrincipal { get; set; }
	}
}
