using RDD.Domain.Helpers;

namespace RDD.Domain
{
    public interface IExecutionModeProvider
    {
        ExecutionMode GetExecutionMode();
    }
}
