using RDD.Domain;

namespace RDD.Infra.Contexts
{
    public class InMemoryExecutionContext : IExecutionContext
    {
        public IPrincipal curPrincipal { get; set; }
    }
}
