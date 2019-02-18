using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.AutoMapper;
using Rdd.Web.Helpers;
using Rdd.Web.Querying;
using Rdd.Web.Tests.Models;
using Xunit;

namespace Rdd.Web.Tests.Services
{
    [Collection("automapper")]
    public class RddAutoMapperBuilderTests
    {
        private readonly AutomapperFixture fixture;

        public RddAutoMapperBuilderTests(AutomapperFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestAutoMapper()
        {
            var services = new ServiceCollection();
            new RddBuilder(services).AddAutoMapper();
            services.TryAddSingleton<IExpressionParser, ExpressionParser>();
            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<IRddObjectsMapper<DTOCat, Cat>>());
            Assert.NotNull(provider.GetRequiredService<IMapper>());
        }
    }
}