using RDD.Domain;
using RDD.Domain.Helpers;

namespace RDD.Infra.Helpers
{
	public class TestExecutionModeProvider : IExecutionModeProvider
	{
		public ExecutionMode GetExecutionMode() { return ExecutionMode.Test; }
	}
}
