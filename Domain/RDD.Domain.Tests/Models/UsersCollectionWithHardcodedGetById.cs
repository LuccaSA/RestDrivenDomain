using System;
using System.Threading.Tasks;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithHardcodedGetById : UsersCollection
    {
        public UsersCollectionWithHardcodedGetById(IRepository<User> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
            : base(repository, execution, combinationsHolder) { }

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
