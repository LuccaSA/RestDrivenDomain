using RDD.Domain.Models;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithParameters : RestCollection<UserWithParameters, int>
    {
        public UsersCollectionWithParameters(IRepository<UserWithParameters> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
            : base(repository, execution, combinationsHolder) { }
    }
}
