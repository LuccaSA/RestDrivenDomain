using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rdd.Domain;
using Rdd.Domain.Json;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using Rdd.Infra.Rights;
using Rdd.Infra.Web.Models;
using Rdd.Web.Helpers;
using Rdd.Web.Querying;
using Rdd.Web.Tests.ServerMock;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests
{
    public class ValidationTests
    {
        [Theory]
        [InlineData(typeof(RestCollection<,>), true)]
        [InlineData(typeof(ValidationOkCollection<,>), true)]
        [InlineData(typeof(ValidationFailCollection<,>), false)]
        [InlineData(typeof(ValidationThrowCollection<,>), false)]
        [InlineData(typeof(ValidationThrowAsyncCollection<,>), false)]
        public async Task ValidOk(Type collectionType, bool modificationExpected)
        {
            // arrange
            var services = new ServiceCollection();
            services
                .AddRdd<ExchangeRateDbContext>()
                .WithDefaultRights(RightDefaultMode.Open);

            services.AddDbContext<ExchangeRateDbContext>((service, options) => { options.UseInMemoryDatabase("validation"); });
            services.AddScoped(typeof(IRestCollection<,>), collectionType);
            var provider = services.BuildServiceProvider();

            var dbContext = provider.GetRequiredService<DbContext>();
            dbContext.Add(new ExchangeRate { Id = 42, Name = "42" });
            await dbContext.SaveChangesAsync();

            // act
            var repository = provider.GetRequiredService<IRepository<ExchangeRate, int>>();
            var collection = provider.GetRequiredService<IRestCollection<ExchangeRate, int>>();

            var candidate = provider.GetRequiredService<ICandidateParser>().Parse<ExchangeRate, int>(@"{ ""name"": ""something""}");

            ExchangeRate ok;
            try
            {
                ok = await collection.UpdateByIdAsync(42, candidate);
            }
            catch (Exception)
            {
                ok = await repository.GetAsync(42);
            }

            // assert
            if (modificationExpected)
            {
                Assert.Equal("something", ok.Name);
            }
            else
            {
                Assert.Equal("42", ok.Name);
            }
        }

        public class ValidationFailCollection<TEntity, TKey> : RestCollection<TEntity, TKey> where TEntity : class, IEntityBase<TKey> where TKey : IEquatable<TKey>
        {
            public ValidationFailCollection(IRepository<TEntity, TKey> repository, IPatcher<TEntity> patcher, IInstanciator<TEntity> instanciator) 
                : base(repository, patcher, instanciator) { }

            protected override Task<bool> ValidateEntityAsync(TEntity entity) => Task.FromResult(false);
        }

        public class ValidationOkCollection<TEntity, TKey> : RestCollection<TEntity, TKey> where TEntity : class, IEntityBase<TKey> where TKey : IEquatable<TKey>
        {
            public ValidationOkCollection(IRepository<TEntity, TKey> repository, IPatcher<TEntity> patcher, IInstanciator<TEntity> instanciator) 
                : base(repository, patcher, instanciator) { }

            protected override Task<bool> ValidateEntityAsync(TEntity entity) => Task.FromResult(true);
        }

        public class ValidationThrowCollection<TEntity, TKey> : RestCollection<TEntity, TKey> where TEntity : class, IEntityBase<TKey> where TKey : IEquatable<TKey>
        {
            public ValidationThrowCollection(IRepository<TEntity, TKey> repository, IPatcher<TEntity> patcher, IInstanciator<TEntity> instanciator) 
                : base(repository, patcher, instanciator) { }

            protected override Task<bool> ValidateEntityAsync(TEntity entity) => throw new NotImplementedException();
        }

        public class ValidationThrowAsyncCollection<TEntity, TKey> : RestCollection<TEntity, TKey> where TEntity : class, IEntityBase<TKey> where TKey : IEquatable<TKey>
        {
            public ValidationThrowAsyncCollection(IRepository<TEntity, TKey> repository, IPatcher<TEntity> patcher, IInstanciator<TEntity> instanciator) 
                : base(repository, patcher, instanciator) { }

            protected override async Task<bool> ValidateEntityAsync(TEntity entity)
            {
                await Task.Delay(1);
                throw new NotImplementedException();
            }
        }
    }
}
