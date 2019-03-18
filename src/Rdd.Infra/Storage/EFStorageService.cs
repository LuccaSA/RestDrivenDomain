using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;

namespace Rdd.Infra.Storage
{
    public class EFStorageService : IStorageService
    {
        protected DbContext DbContext { get; }

        public EFStorageService(DbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public virtual IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            return DbContext.Set<TEntity>();
        }

        //https://expertcodeblog.wordpress.com/2018/02/19/net-core-2-0-resolve-error-the-source-iqueryable-doesnt-implement-iasyncenumerable/
        public virtual Task<IEnumerable<TEntity>> EnumerateEntitiesAsync<TEntity>(IQueryable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            return EnumerateInternalAsync(entities, cancellationToken);
        }

        private async Task<IEnumerable<TEntity>> EnumerateInternalAsync<TEntity>(IQueryable<TEntity> entities, CancellationToken cancellationToken)
        {
            if (!(entities is IAsyncEnumerable<TEntity>))
            {
                return entities.ToList();
            }

            return await entities.ToListAsync(cancellationToken);
        }

        public virtual Task<int> CountAsync<TEntity>(IQueryable<TEntity> entities, CancellationToken cancellationToken = default)
             where TEntity : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            return CountInternalAsync(entities, cancellationToken);
        }

        public virtual Task<bool> AnyAsync<TEntity>(IQueryable<TEntity> entities, CancellationToken cancellationToken = default) 
            where TEntity : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            return entities.AnyAsync(cancellationToken);
        }

        private async Task<int> CountInternalAsync<TEntity>(IQueryable<TEntity> entities, CancellationToken cancellationToken)
        {
            if (!(entities is IAsyncEnumerable<TEntity>))
            {
                return entities.Count();
            }

            return await entities.CountAsync(cancellationToken);
        }

        public virtual void Add<TEntity>(TEntity entity)
            where TEntity : class
        {
            DbContext.Set<TEntity>().Add(entity);
        }

        public virtual void AddRange<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            DbContext.Set<TEntity>().AddRange(entities);
        }

        public virtual void Remove<TEntity>(TEntity entity)
            where TEntity : class
        {
            DbContext.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            DbContext.Set<TEntity>().RemoveRange(entities);
        }

        public void DiscardChanges<TEntity>(TEntity entity)
            where TEntity : class
        {
            if (entity == null)
            {
                return;
            }

            EntityEntry<TEntity> entry = DbContext.Entry(entity);
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.State = EntityState.Unchanged;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                    entry.State = EntityState.Unchanged;
                    break;
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
            }
        }

        public virtual Task<int> ExecuteScriptAsync(string script, CancellationToken cancellationToken = default)
        {
            return DbContext.Database.ExecuteSqlCommandAsync(script, cancellationToken);
        }
    }
}