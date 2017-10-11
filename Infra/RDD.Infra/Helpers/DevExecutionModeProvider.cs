using RDD.Domain;
using RDD.Domain.Helpers;

namespace RDD.Infra.Helpers
{
    public class DevExecutionModeProvider : IExecutionModeProvider
    {
        public ExecutionMode GetExecutionMode() { return ExecutionMode.Dev; }
    }
}
