using Microsoft.Extensions.DependencyInjection;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Json;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using Rdd.Web.Helpers;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Tests.ServerMock;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rdd.Web.Tests.Services
{
    public class ServicesCollectionTests
    {
        public abstract class Hierarchy2 : IEntityBase<int>
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string Type { get; set; }
            public int Id { get; set; }

            public Hierarchy2 Clone() => this;
            public object GetId() => Id;
            public void SetId(object id) => Id = (int)id;
        }

        public class Super : Hierarchy2
        {
        }

        public class InheritanceConfiguration2 : IInheritanceConfiguration<Hierarchy2>
        {
            public Type BaseType => typeof(Hierarchy2);

            public string Discriminator => "type";

            public IReadOnlyDictionary<string, Type> Mappings => new Dictionary<string, Type>
            {
                { "super", typeof(Super) }
            };
        }

        [Fact]
        public void TestInheritanceRegister()
        {
            var services = new ServiceCollection();
            var setup = new RddBuilder(services);

            setup.AddRddInheritanceConfiguration<InheritanceConfiguration, Hierarchy, int>(new InheritanceConfiguration());
            setup.AddRddInheritanceConfiguration<InheritanceConfiguration2, Hierarchy2, int>(new InheritanceConfiguration2());

            var provider = services.BuildServiceProvider();

            var configs = provider.GetRequiredService<IEnumerable<IInheritanceConfiguration>>();

            Assert.Equal(2, configs.ToList().Count);

            var config1 = provider.GetRequiredService<IInheritanceConfiguration<Hierarchy>>();
            var config2 = provider.GetRequiredService<IInheritanceConfiguration<Hierarchy2>>();
        }

        [Fact]
        public void TestEmptyInheritanceRegister()
        {
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var configs = provider.GetRequiredService<IEnumerable<IInheritanceConfiguration>>();

            Assert.Empty(configs);
        }

        [Fact]
        public void TestRddSerializationRegister()
        {
            var services = new ServiceCollection();
            var setup = new RddBuilder(services);
            setup.AddRddSerialization();

            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<ISerializerProvider>());
        }

        [Fact]
        public void TestRddRegister()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ExchangeRateDbContext>();//necessary or .AddRdd fails
            services.AddRdd<ExchangeRateDbContext>();

            services.AddScoped(typeof(IRightExpressionsHelper<>),typeof(OpenRightExpressionsHelper<>));
            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<IUnitOfWork>());
            Assert.NotNull(provider.GetRequiredService<IStorageService>());
            Assert.NotNull(provider.GetRequiredService<IPatcherProvider>());
            Assert.NotNull(provider.GetRequiredService<IJsonParser>());
            Assert.NotNull(provider.GetRequiredService<IStringConverter>());
            Assert.NotNull(provider.GetRequiredService<IExpressionParser>());
            Assert.NotNull(provider.GetRequiredService<ICandidateParser>());

            Assert.NotNull(provider.GetRequiredService<IReadOnlyRepository<ExchangeRate>>());
            Assert.NotNull(provider.GetRequiredService<IRepository<ExchangeRate>>());
            Assert.NotNull(provider.GetRequiredService<IPatcher<ExchangeRate>>());
            Assert.NotNull(provider.GetRequiredService<IInstanciator<ExchangeRate>>());
            Assert.NotNull(provider.GetRequiredService<IReadOnlyRestCollection<ExchangeRate, int>>());
            Assert.NotNull(provider.GetRequiredService<IRestCollection<ExchangeRate, int>>());
            Assert.NotNull(provider.GetRequiredService<IReadOnlyAppController<ExchangeRate, int>>());
            Assert.NotNull(provider.GetRequiredService<IAppController<ExchangeRate, int>>());
        }
    }
}
