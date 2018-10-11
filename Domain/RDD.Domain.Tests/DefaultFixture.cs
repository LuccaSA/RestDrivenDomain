using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System;

namespace Rdd.Domain.Tests
{
    public class DefaultFixture : IDisposable
    {
        public IRightExpressionsHelper<User> RightsService { get; private set; }
        public IPatcherProvider PatcherProvider { get; private set; }
        public IInstanciator<User> Instanciator { get; private set; }
        public InMemoryStorageService InMemoryStorage { get; private set; }
        public IRepository<User> UsersRepo { get; private set; }

        public DefaultFixture()
        {
            RightsService = new OpenRightExpressionsHelper<User>();
            PatcherProvider = new PatcherProvider();
            Instanciator = new DefaultInstanciator<User>();
            InMemoryStorage = new InMemoryStorageService();
            UsersRepo = new Repository<User>(InMemoryStorage, RightsService);
        }

        public void Dispose() { }
    }
}
