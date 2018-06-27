using RDD.Domain.Models;
using RDD.Domain.Patchers;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollection : RestCollection<User, int>
    {
        public UsersCollection(IRepository<User> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder, IPatcherProvider patcherProvider)
            : base(repository, execution, combinationsHolder, patcherProvider) { }
    }
}
