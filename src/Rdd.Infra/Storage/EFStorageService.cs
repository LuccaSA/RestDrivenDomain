﻿using Microsoft.EntityFrameworkCore;
using Rdd.Application;
using Rdd.Infra.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Rdd.Infra.Storage
{
    public class EFStorageService : IStorageService, IUnitOfWork
    {
        protected DbContext DbContext { get; }

        public EFStorageService(DbContext dbContext)
        {
            DbContext = dbContext;
        }

        public virtual IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            return DbContext.Set<TEntity>();
        }

        /// <summary>
        /// source : https://expertcodeblog.wordpress.com/2018/02/19/net-core-2-0-resolve-error-the-source-iqueryable-doesnt-implement-iasyncenumerable/
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> EnumerateEntitiesAsync<TEntity>(IQueryable<TEntity> entities)
            where TEntity : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            //IQueryable is not coming from an EF DbSet !
            if (!(entities is IAsyncEnumerable<TEntity>))
            {
                return entities.ToList();
            }

            return await entities.ToListAsync();
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
            EntityEntry<TEntity> entry = DbContext.Entry(entity);
            if (entry == null)
            {
                return;
            }
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
                case EntityState.Detached:
                case EntityState.Unchanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry.State), "Unknown EntityState");
            }
        }

        public virtual async Task SaveChangesAsync()
        {
            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                switch (ex.InnerException?.InnerException)
                {
                    case ArgumentException ae:
                        throw ae;
                    case SqlException se:
                        switch (se.Number)
                        {
                            case 2627:
                                throw new SqlUniqConstraintException(se.Message);
                            default:
                                throw se;
                        }
                    default:
                        throw ex.InnerException ?? ex;
                }
            }
        }

        public virtual Task<int> ExecuteScriptAsync(string script)
        {
            return DbContext.Database.ExecuteSqlCommandAsync(script);
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}