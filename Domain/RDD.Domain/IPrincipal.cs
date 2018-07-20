using RDD.Domain.Helpers;

namespace RDD.Domain
{
    public interface IPrincipal
    {
        int Id { get; }
        string Token { get; set; }
        string Name { get; }
        Culture Culture { get; }
    }
}
