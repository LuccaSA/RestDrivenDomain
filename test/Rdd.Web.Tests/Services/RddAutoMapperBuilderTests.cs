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
        private readonly AutomapperFixture _fixture;

        public RddAutoMapperBuilderTests(AutomapperFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestAutoMapper()
        {
            var services = new ServiceCollection();
            new RddBuilder(services).AddAutoMapper();
            services.AddSingleton(_fixture.Mapper);
            services.TryAddSingleton<IExpressionParser, ExpressionParser>();
            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<IRddObjectsMapper<DTOCat, Cat>>());
        }
    }
}