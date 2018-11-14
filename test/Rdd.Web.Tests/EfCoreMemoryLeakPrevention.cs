using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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
            services.AddDbContext<UselessDbContext>((service, options) =>
            {
                options.UseInMemoryDatabase("leaktest");
            });

            var provider = services.BuildServiceProvider();

            var dbContext = provider.GetRequiredService<UselessDbContext>();

            for (int i = 0; i < 1000; i++)
            {
                var builder = new WebFilterConverter<UselessEntity>();
                var expression = builder.Equals(PropertyExpression<UselessEntity>.New(u => u.Id), new List<int> { i });
                var tmp = await dbContext.UselessEntities.Where(expression).ToListAsync();
                Assert.Empty(tmp);
            }

            int cachedQueryCount = 0;

            var cache = provider.GetRequiredService<IMemoryCache>() as MemoryCache;
            var entriesProperty = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var entries = entriesProperty.GetValue(cache) as ICollection;// as ConcurrentDictionary<object, ICacheEntry>;
            var items = new List<string>();
            if (entries != null)
            {
                foreach (var item in entries)
                {
                    var methodInfoVal = item.GetType().GetProperty("Value");
                    var val = methodInfoVal.GetValue(item) as ICacheEntry;
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

            Assert.Equal(1, cachedQueryCount);
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
