using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Patchers;
using System;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithParametersAndOverride : RestCollection<UserWithParameters, int>
    {
        public UsersCollectionWithParametersAndOverride(IRepository<UserWithParameters> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder, IPatcherProvider patcherProvider)
            : base(repository, execution, combinationsHolder, patcherProvider) { }

        public override UserWithParameters InstanciateEntity(ICandidate<UserWithParameters, int> candidate)
        {
            var id = candidate.Value.Id;
            var name = candidate.Value.Name;

            return new UserWithParameters(id, name);
        }
    }
}
