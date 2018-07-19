using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithParameters : RestCollection<UserWithParameters, int>
    {
        public UsersCollectionWithParameters(IRepository<UserWithParameters> repository, IRightsService rightsService, IPatcherProvider patcherProvider)
            : base(repository, rightsService, patcherProvider) { }
    }
}
