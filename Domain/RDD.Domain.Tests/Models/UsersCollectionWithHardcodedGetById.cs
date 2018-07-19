using RDD.Domain.Models.Querying;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using System.Threading.Tasks;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithHardcodedGetById : UsersCollection
    {
        public UsersCollectionWithHardcodedGetById(IRepository<User> repository, IRightsService rightsService, IPatcherProvider patcherProvider)
            : base(repository, rightsService, patcherProvider) { }

        public override Task<User> GetByIdAsync(int id, Query<User> query)
        {
            //Only for get, because PUT will always make a GetById( ) to retrieve the entity to update
            if (query.Verb == Helpers.HttpVerbs.Get)
            {
                return Task.FromResult(new User
                {
                    Id = 4
                });
            }

            return base.GetByIdAsync(id, query);
        }
    }
}
