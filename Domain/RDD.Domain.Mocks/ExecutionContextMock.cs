using Moq;

namespace RDD.Domain.Mocks
{
    public class ExecutionContextMock : Mock<IExecutionContext>, IExecutionContext
    {
        public IPrincipal curPrincipal { get; set; }

        public ExecutionContextMock()
        {
            curPrincipal = new PrincipalMock();
        }
    }
}
