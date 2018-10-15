using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using System;
using System.Threading.Tasks;

namespace Rdd.Domain.Tests.Models
{
    public class UsersCollectionWithHardcodedGetById : UsersCollection
    {
        public UsersCollectionWithHardcodedGetById(IRepository<User> repository, IPatcherProvider patcherProvider, IInstanciator<User> instanciator)
            : base(repository, patcherProvider, instanciator) { }

        public override Task<User> GetByIdAsync(Guid id, Query<User> query)
        {
            //Only for get, because PUT will always make a GetById( ) to retrieve the entity to update
            if (query.Verb == Helpers.HttpVerbs.Get)
            {
                return Task.FromResult(new User
                {
                    Id = Guid.NewGuid()
                });
            }

            return base.GetByIdAsync(id, query);
        }
    }
}
