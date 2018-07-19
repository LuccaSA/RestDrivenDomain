using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithOverride : RestCollection<User, int>
    {
        public UsersCollectionWithOverride(IRepository<User> repository, IRightsService rightsService, IPatcherProvider patcherProvider)
            : base(repository, rightsService, patcherProvider) { }

        public override User InstanciateEntity(ICandidate<User, int> candidate)
        {
            return new User();
        }
    }
}
