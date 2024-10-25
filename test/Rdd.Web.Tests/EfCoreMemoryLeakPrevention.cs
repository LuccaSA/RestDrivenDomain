using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Infra.Helpers;
using Xunit;

namespace Rdd.Web.Tests
{
    public class EfCoreMemoryLeakPrevention
    {
        [Fact]
        public async Task GenerateMultipleQueries()
        {
            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();
            services.AddDbContext<UselessDbContext>((service, options) => options.UseInMemoryDatabase("leaktest_3_0"));

            var provider = services.BuildServiceProvider();

            var dbContext = provider.GetRequiredService<UselessDbContext>();

            var builder = new WebFilterConverter<UselessEntity>();
            var initExpr = PropertyExpression<UselessEntity>.New(u => u.Id);

            var expressions = Enumerable.Range(0, 1000).Select(i => builder.Equals(initExpr, new List<int> { i })).ToArray();

            var tmp = await dbContext.UselessEntities.Where(expressions[0]).ToListAsync();//Warmup
            for (int i = 0; i < 1000; i++)
            {
                tmp = await dbContext.UselessEntities.Where(expressions[i]).ToListAsync();
                Assert.Empty(tmp);
            }

#pragma warning disable EF1001 // Internal EF Core API usage.
            var efServiceProviders = ServiceProviderCache.Instance;

            var efConfigs = typeof(ServiceProviderCache).GetField("_configurations", BindingFlags.NonPublic | BindingFlags.Instance);
            var localServiceProvider = efConfigs.GetValue(efServiceProviders) as ConcurrentDictionary<IDbContextOptions, ValueTuple<IServiceProvider, IDictionary<string, string>>>;
            var localCache = localServiceProvider.Values.First().Item1.GetService<ICompiledQueryCache>() as CompiledQueryCache;
            var cacheProperty = typeof(CompiledQueryCache).GetField("_memoryCache", BindingFlags.NonPublic | BindingFlags.Instance);
            var cache = cacheProperty.GetValue(localCache) as MemoryCache;
#pragma warning restore EF1001 // Internal EF Core API usage.

            int cachedQueryCount = 0;

            var coherentStateProperty = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
            var state = coherentStateProperty.GetValue(cache);
            var items = new List<string>();
            var allEntries = state.GetType().GetMethod("GetAllValues").Invoke(state, Array.Empty<object>()) as IEnumerable;
            if (allEntries != null)
            {
                foreach (ICacheEntry val in allEntries)
                {
                    if (val?.Value == null)
                    {
                        continue;
                    }
                    var contentType = val.Value.GetType();
                    var gens = contentType.GenericTypeArguments;
                    if (gens.Length == 2 && gens[0] == typeof(QueryContext) && gens[1] == typeof(IAsyncEnumerable<UselessEntity>))
                    {
                        cachedQueryCount++;
                    }
                }
            }

            Assert.True(cachedQueryCount <= 1);// is either one or zero, but doesn't matter, that's not what we are testing.
        }
    }

    public class UselessEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UselessDbContext : DbContext
    {
        public UselessDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<UselessEntity> UselessEntities { get; set; }
    }
}