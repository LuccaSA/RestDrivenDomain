using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using Rdd.Infra.Web.Models;
using System;
using System.Threading.Tasks;

namespace Rdd.Domain.Tests.Models
{
    public class UsersCollectionWithHardcodedGetById : UsersCollection
    {
        public UsersCollectionWithHardcodedGetById(IRepository<User, Guid> repository, IPatcherProvider patcherProvider, IInstanciator<User> instanciator)
            : base(repository, patcherProvider, instanciator) { }

        public override Task<User> GetByIdAsync(Guid id, IQuery<User> query)
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
