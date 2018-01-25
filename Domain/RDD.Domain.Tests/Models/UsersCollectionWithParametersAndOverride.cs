using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithParametersAndOverride : RestCollection<UserWithParameters, int>
    {
        public UsersCollectionWithParametersAndOverride(IRepository<UserWithParameters> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
            : base(repository, execution, combinationsHolder) { }

        public override UserWithParameters InstanciateEntity(PostedData datas)
        {
            var id = Convert.ToInt32(datas["id"].Value);
            var name = datas["name"].Value;

            return new UserWithParameters(id, name);
        }
    }
}
