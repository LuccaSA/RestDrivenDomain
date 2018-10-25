using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using Rdd.Infra.Rights;
using Rdd.Infra.Storage;
using Rdd.Infra.Web.Models;
using System;
using System.Threading.Tasks;

namespace Rdd.Domain.Tests.Models
{
    public class UsersRepositoryWithHardcodedGetById : OpenRepository<User, Guid>
    {
        public UsersRepositoryWithHardcodedGetById(IStorageService storageService, IRightExpressionsHelper<User> rightsService, HttpQuery<User, Guid> httpQuery)
            : base(storageService, rightsService, httpQuery) { }

        public override Task<User> GetAsync(Guid id)
        {
            if (HttpQuery.Verb == Helpers.HttpVerbs.Get)
            {
                return Task.FromResult(new User
                {
                    Id = Guid.NewGuid()
                });
            }

            return base.GetAsync(id);
        }
    }
}
