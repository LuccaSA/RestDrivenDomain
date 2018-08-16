using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace RDD.Infra.Storage
{
    public class MonoContextResolver<TDbContext> : IDbContextResolver
        where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public MonoContextResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public DbContext GetMatchingContext<TEntity>() => _serviceProvider.GetRequiredService<TDbContext>();
    }
}