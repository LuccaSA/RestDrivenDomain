using System.Threading.Tasks;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;

namespace Rdd.Domain.Tests.Models
{
    public class UsersCollectionWithParameters : RestCollection<UserWithParameters, int>
    {
        private readonly IInstantiator<UserWithParameters> _instanciator;

        public UsersCollectionWithParameters(IRepository<UserWithParameters, int> repository, IPatcherProvider patcherProvider, IInstantiator<UserWithParameters> instanciator)
            : base(repository, new ObjectPatcher<UserWithParameters>(patcherProvider, new ReflectionHelper()))
        {
            _instanciator = instanciator;
        }

        public override Task<UserWithParameters> InstantiateEntityAsync(ICandidate<UserWithParameters, int> candidate)
        {
            return _instanciator.InstantiateAsync(candidate);
        }
    }
}
