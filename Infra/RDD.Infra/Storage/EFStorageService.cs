using Microsoft.EntityFrameworkCore;
using RDD.Infra.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra.Storage
{
    public class EFStorageService<TEntity> : IStorageService<TEntity>
            where TEntity : class
    {
        protected IDbContextResolver DbContextResolver { get; }
        protected Queue<Task> AfterSaveChangesActions { get; }

        public EFStorageService(IDbContextResolver dbContextResolver)
        {
            DbContextResolver = dbContextResolver;
            AfterSaveChangesActions = new Queue<Task>();
        }

        private DbContext GetDbContext()
        {
            return DbContextResolver.GetMatchingContext<TEntity>();
        }

        IQueryable<TEntity> IStorageService<TEntity>.Set() => Set();
        public virtual DbSet<TEntity> Set()
        {
            return GetDbContext().Set<TEntity>();
        }

        /// <summary>
        /// source : https://expertcodeblog.wordpress.com/2018/02/19/net-core-2-0-resolve-error-the-source-iqueryable-doesnt-implement-iasyncenumerable/
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> EnumerateEntitiesAsync(IQueryable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            //IQueryable is not coming from an EF DbSet !
            if (!(entities is IAsyncEnumerable<TEntity>))
                return entities.ToList();

            return await entities.ToListAsync();
        }

        public virtual void Add(TEntity entity)
        {
            Set().Add(entity);
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            Set().AddRange(entities);
        }

        public virtual void Remove(TEntity entity)
        {
            Set().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Set().RemoveRange(entities);
        }

        public void AddAfterSaveChangesAction(Task action)
        {
            AfterSaveChangesActions.Enqueue(action);
        }

        public virtual async Task SaveChangesAsync()
        {
            try
            {
                await GetDbContext().SaveChangesAsync();

                while (AfterSaveChangesActions.Count > 0)
                {
                    await AfterSaveChangesActions.Dequeue();
                }
            }
            catch (DbUpdateException ex)
            {
                var updateException = ex.InnerException;

                if (updateException.InnerException is ArgumentException)
                {
                    throw updateException.InnerException;
                }
                else if (updateException.InnerException is SqlException)
                {
                    var sqlException = (SqlException)updateException.InnerException;

                    switch (sqlException.Number)
                    {
                        case 2627:
                            throw new SqlUniqConstraintException(sqlException.Message);
                        default:
                            throw sqlException;
                    }
                }
                else
                {
                    throw updateException;
                }
            }
        }

        public async Task<string> ExecuteScriptAsync(string script)
        {
            return (await GetDbContext().Database.ExecuteSqlCommandAsync(script)).ToString();
        }
    }
}