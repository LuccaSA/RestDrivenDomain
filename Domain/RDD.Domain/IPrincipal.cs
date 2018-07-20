using RDD.Domain.Helpers;

namespace RDD.Domain
{
    public enum PrincipalType
    {
        User = 0,
        ApiKey = 1,
        WebService = 2
    }

    public interface IPrincipal
    {
        int Id { get; }
        string Token { get; set; }
        string Name { get; }
        Culture Culture { get; }

        PrincipalType Type { get; }
    }
}
