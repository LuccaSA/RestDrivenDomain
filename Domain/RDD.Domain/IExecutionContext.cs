namespace RDD.Domain
{
    public interface IExecutionContext
    {
        IPrincipal curPrincipal { get; set; }
    }
}
