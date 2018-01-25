using RDD.Domain.Models;
using RDD.Domain.Models.Querying;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithOverride : RestCollection<User, int>
    {
        public UsersCollectionWithOverride(IRepository<User> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
            : base(repository, execution, combinationsHolder) { }

        public override User InstanciateEntity(PostedData datas)
        {
            return new User();
        }
    }
}
