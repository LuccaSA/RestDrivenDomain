using RDD.Domain.Models;
using RDD.Domain.Patchers;
using System;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollection : RestCollection<User, Guid>
    {
        public UsersCollection(IRepository<User> repository, IPatcherProvider patcherProvider, IInstanciator<User> instanciator)
            : base(repository, new ObjectPatcher<User>(patcherProvider), instanciator) { }
    }
}
