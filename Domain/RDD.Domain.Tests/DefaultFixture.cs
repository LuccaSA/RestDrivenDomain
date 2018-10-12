using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rdd.Application;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Mocks;
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
        public IServiceProvider ServiceProvider { get; private set; }
        public IRightExpressionsHelper<User> RightsService { get; private set; }
        public IPatcherProvider PatcherProvider => ServiceProvider.GetService<IPatcherProvider>();
        public IReflectionProvider ReflectionProvider => ServiceProvider.GetService<IReflectionProvider>();
        public IInstanciator<User> Instanciator { get; private set; }
        public InMemoryStorageService InMemoryStorage { get; private set; }
        public IRepository<User> UsersRepo { get; private set; }

        public DefaultFixture()
        {
            var services = new ServiceCollection();

            services.TryAddSingleton<IReflectionProvider, ReflectionProvider>();
            services.TryAddSingleton<IPatcherProvider, PatcherProvider>();
            services.TryAddSingleton<EnumerablePatcher>();
            services.TryAddSingleton<DictionaryPatcher>();
            services.TryAddSingleton<ValuePatcher>();
            services.TryAddSingleton<DynamicPatcher>();
            services.TryAddSingleton<ObjectPatcher>();

            ServiceProvider = services.BuildServiceProvider();

            RightsService = new RightsServiceMock<User>();
            Instanciator = new DefaultInstanciator<User>();
            InMemoryStorage = new InMemoryStorageService();
            UsersRepo = new Repository<User>(InMemoryStorage, RightsService);
        }

        public void Dispose() { }
    }
}
