using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollection : RestCollection<User, int>
    {
        public UsersCollection(IRepository<User> repository, IRightsService rightsService, IPatcherProvider patcherProvider)
            : base(repository, rightsService, patcherProvider) { }
    }
}
