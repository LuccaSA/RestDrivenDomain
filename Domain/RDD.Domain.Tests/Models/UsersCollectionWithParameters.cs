using RDD.Domain.Models;
using RDD.Domain.Patchers;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithParameters : RestCollection<UserWithParameters, int>
    {
        public UsersCollectionWithParameters(IRepository<UserWithParameters> repository,
            IPatcherProvider patcherProvider, IInstanciator<UserWithParameters> instanciator)
            : base(repository, new ObjectPatcher<UserWithParameters>(patcherProvider), instanciator)
        {
        }
    }
}
