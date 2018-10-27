using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using System;

namespace Rdd.Domain.Tests.Models
{
    public class UsersCollection : RestCollection<User, Guid>
    {
        public UsersCollection(IRepository<User, Guid> repository, IPatcherProvider patcherProvider, IInstanciator<User> instanciator)
            : base(repository, new ObjectPatcher<User>(patcherProvider, new ReflectionHelper()), instanciator) { }
    }
}
