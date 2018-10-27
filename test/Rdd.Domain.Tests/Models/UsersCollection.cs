using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using System;
using System.Threading.Tasks;

namespace Rdd.Domain.Tests.Models
{
    public class UsersCollection : RestCollection<User, Guid>
    {
        private readonly IInstantiator<User> _instanciator;

        public UsersCollection(IRepository<User, Guid> repository, IPatcher<User> patcher, IInstantiator<User> instanciator)
            : base(repository, patcher)
        {
            _instanciator = instanciator;
        }

        public UsersCollection(IRepository<User, Guid> repository, IPatcherProvider patcherProvider, IInstantiator<User> instanciator)
            : this(repository, new ObjectPatcher<User>(patcherProvider, new ReflectionHelper()), instanciator) { }

        public override Task<User> InstantiateEntityAsync(ICandidate<User, Guid> candidate)
        {
            return _instanciator.InstantiateAsync(candidate);
        }
    }
}
