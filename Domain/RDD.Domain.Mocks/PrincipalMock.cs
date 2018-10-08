using Rdd.Domain.Helpers;

namespace Rdd.Domain.Mocks
{
    public class PrincipalMock : IPrincipal
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string Name { get; set; }
        public Culture Culture { get; }

        public PrincipalType Type => PrincipalType.User;
    }
}
