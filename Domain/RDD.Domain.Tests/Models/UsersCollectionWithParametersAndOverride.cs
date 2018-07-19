using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithParametersAndOverride : RestCollection<UserWithParameters, int>
    {
        public UsersCollectionWithParametersAndOverride(IRepository<UserWithParameters> repository, IRightsService rightsService, IPatcherProvider patcherProvider)
            : base(repository, rightsService, patcherProvider) { }

        public override UserWithParameters InstanciateEntity(ICandidate<UserWithParameters, int> candidate)
        {
            var id = candidate.Value.Id;
            var name = candidate.Value.Name;

            return new UserWithParameters(id, name);
        }
    }
}
