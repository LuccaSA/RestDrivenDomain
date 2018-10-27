using Microsoft.Extensions.DependencyInjection;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Json;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using Rdd.Infra.Storage;
using Rdd.Web.Helpers;
using Rdd.Web.Querying;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Tests.ServerMock;
using Xunit;

namespace Rdd.Web.Tests.Services
{
    public class ServicesCollectionTests
    {
        [Fact]
        public void TestRddRegister()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ExchangeRateDbContext>();//necessary or .AddRdd fails
            services.AddRdd<ExchangeRateDbContext>();

            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<IUnitOfWork>());
            Assert.NotNull(provider.GetRequiredService<IStorageService>());
            Assert.NotNull(provider.GetRequiredService<IPatcherProvider>());
            Assert.NotNull(provider.GetRequiredService<IJsonParser>());
            Assert.NotNull(provider.GetRequiredService<IStringConverter>());
            Assert.NotNull(provider.GetRequiredService<IExpressionParser>());
            Assert.NotNull(provider.GetRequiredService<ICandidateParser>());

            Assert.NotNull(provider.GetRequiredService<ISerializerProvider>());

            Assert.NotNull(provider.GetRequiredService<IReadOnlyRepository<ExchangeRate, int>>());
            Assert.NotNull(provider.GetRequiredService<IRepository<ExchangeRate, int>>());
            Assert.NotNull(provider.GetRequiredService<IPatcher<ExchangeRate>>());
            Assert.NotNull(provider.GetRequiredService<IInstanciator<ExchangeRate>>());
            Assert.NotNull(provider.GetRequiredService<IReadOnlyRestCollection<ExchangeRate, int>>());
            Assert.NotNull(provider.GetRequiredService<IRestCollection<ExchangeRate, int>>());
            Assert.NotNull(provider.GetRequiredService<IReadOnlyAppController<ExchangeRate, int>>());
            Assert.NotNull(provider.GetRequiredService<IAppController<ExchangeRate, int>>());
        }
    }
}