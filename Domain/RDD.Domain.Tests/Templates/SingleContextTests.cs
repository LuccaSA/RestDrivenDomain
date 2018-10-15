using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rdd.Application;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System;

namespace Rdd.Domain.Tests.Templates
{
    public class SingleContextTests
    {
        protected Func<string, IStorageService> _newStorage;
        protected IRightExpressionsHelper<User> _rightsService;
        protected IPatcherProvider _patcherProvider;
        protected IInstanciator<User> Instanciator { get; set; }

        public SingleContextTests()
        {
            var services = new ServiceCollection();

            services.TryAddSingleton<IReflectionHelper, ReflectionHelper>();
            services.TryAddSingleton<IPatcherProvider, PatcherProvider>();
            services.TryAddSingleton<EnumerablePatcher>();
            services.TryAddSingleton<DictionaryPatcher>();
            services.TryAddSingleton<ValuePatcher>();
            services.TryAddSingleton<DynamicPatcher>();
            services.TryAddSingleton<ObjectPatcher>();

            var provider = services.BuildServiceProvider();

            _newStorage = name => new EFStorageService(new DataContext(GetOptions(name)));
            _patcherProvider = new PatcherProvider(provider, provider.GetService<IReflectionHelper>());
            _rightsService = new OpenRightExpressionsHelper<User>();
            Instanciator = new DefaultInstanciator<User>();
        }

        private DbContextOptions<DataContext> GetOptions(string name)
        {
            return new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: name)
                //                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;
        }
    }
}
