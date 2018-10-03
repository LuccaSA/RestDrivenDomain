using RDD.Application;
using RDD.Domain.Mocks;
using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using RDD.Domain.Tests.Models;
using RDD.Infra.Storage;
using System;

namespace RDD.Domain.Tests
{
    public class DefaultFixture : IDisposable
    {
        public IRightExpressionsHelper<User> RightsService { get; private set; }
        public IPatcherProvider PatcherProvider { get; private set; }
        public IInstanciator<User> Instanciator { get; private set; }
        public IStorageService InMemoryStorage { get; private set; }
        public IRepository<User> UsersRepo { get; private set; }

        public DefaultFixture()
        {
            RightsService = new RightsServiceMock<User>();
            PatcherProvider = new PatcherProvider();
            Instanciator = new DefaultInstanciator<User>();
            InMemoryStorage = new InMemoryStorageService();
            UsersRepo = new Repository<User>(InMemoryStorage, RightsService);
        }

        public void Dispose() { }
    }
}
