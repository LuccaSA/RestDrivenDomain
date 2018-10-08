using Rdd.Domain;
using Rdd.Domain.Helpers;

namespace Rdd.Web.Tests.ServerMock
{
    public class CurPrincipal : IPrincipal
    {
        public int Id { get; }
        public string Token { get; set; }
        public string Name { get; }
        public Culture Culture { get; }

        public PrincipalType Type => PrincipalType.User;
    }
}