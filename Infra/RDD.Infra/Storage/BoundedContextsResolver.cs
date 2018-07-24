using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RDD.Infra.Storage
{
    public class BoundedContextsResolver : IDbContextResolver
    {
        private static readonly ConcurrentDictionary<Type, Type> DbContextesByType = new ConcurrentDictionary<Type, Type>();
        private static object _locker = new object();

        public static List<Type> DbContextTypes = new List<Type>();

        private readonly IServiceProvider _serviceProvider;

        public BoundedContextsResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public DbContext GetMatchingContext<TEntity>()
        {
            if (DbContextesByType.Count == 0)
            {
                lock (_locker)
                {
                    if (DbContextesByType.Count == 0)
                    {
                        SetupBoundedContextBinding();
                    }
                }
            }

            DbContextesByType.TryGetValue(typeof(TEntity), out var dbContextType);
            return dbContextType == null ? null : _serviceProvider.GetRequiredService(dbContextType) as DbContext;
        }

        private void SetupBoundedContextBinding()
        {
            if (DbContextTypes != null)
            {
                foreach (var dbContextType in DbContextTypes)
                {
                    var dbContext = _serviceProvider.GetRequiredService(dbContextType) as DbContext;
                    if (dbContext != null)
                    {
                        foreach (var entityType in dbContext.Model.GetEntityTypes())
                        {
                            DbContextesByType.TryAdd(entityType.ClrType, dbContextType);
                        }
                    }
                }
            }
        }
    }
}