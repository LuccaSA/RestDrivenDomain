using System.Diagnostics;

namespace RDD.Domain
{
	public interface IExecutionContext
	{
		IPrincipal curPrincipal { get; set; }
		Stopwatch serverWatch { get; }
		Stopwatch queryWatch { get; }
	}
}
