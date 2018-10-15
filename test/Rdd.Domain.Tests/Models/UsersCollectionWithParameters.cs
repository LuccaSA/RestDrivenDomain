using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;

namespace Rdd.Domain.Tests.Models
{
    public class UsersCollectionWithParameters : RestCollection<UserWithParameters, int>
    {
        public UsersCollectionWithParameters(IRepository<UserWithParameters> repository, IPatcherProvider patcherProvider, IInstanciator<UserWithParameters> instanciator)
            : base(repository, new ObjectPatcher<UserWithParameters>(patcherProvider, new ReflectionHelper()), instanciator) { }
    }
}
