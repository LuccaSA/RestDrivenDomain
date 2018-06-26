using RDD.Domain.Models;
using RDD.Domain.Patchers;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithParameters : RestCollection<UserWithParameters, int>
    {
        public UsersCollectionWithParameters(IRepository<UserWithParameters> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder, IPatcherProvider patcherProvider)
            : base(repository, execution, combinationsHolder, patcherProvider) { }
    }
}
