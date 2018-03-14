using RDD.Domain;
using RDD.Infra.Storage;
using RDD.Infra.Tests.Models;

namespace RDD.Infra.Tests.Repositories
{
    public class UsersRepository : Repository<User>
    {
        public UsersRepository(IStorageService storageService, IExecutionContext executionContext, ICombinationsHolder combinationsHolder)
            : base(storageService, executionContext, combinationsHolder) { }
    }
}
