using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Patchers;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithOverride : RestCollection<User, int>
    {
        public UsersCollectionWithOverride(IRepository<User> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder, IPatcherProvider patcherProvider)
            : base(repository, execution, combinationsHolder, patcherProvider) { }

        public override User InstanciateEntity(ICandidate<User, int> candidate)
        {
            return new User();
        }
    }
}
