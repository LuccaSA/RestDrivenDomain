using Microsoft.EntityFrameworkCore;
using RDD.Domain;
using RDD.Infra.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra.Storage
{
    public class EFStorageService : IStorageService
    {
        protected DbContext DbContext { get; }
        protected Queue<Task> AfterSaveChangesActions { get; }

        public EFStorageService(DbContext dbContext)
        {
            DbContext = dbContext;
            AfterSaveChangesActions = new Queue<Task>();
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
        public virtual async Task<IReadOnlyCollection<TEntity>> EnumerateEntitiesAsync<TEntity>(IQueryable<TEntity> entities)
            where TEntity : class
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            //IQueryable is not coming from an EF DbSet !
            if (!(entities is IAsyncEnumerable<TEntity>))
                return entities.ToList();

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

        public void AddAfterSaveChangesAction(Task action)
        {
            AfterSaveChangesActions.Enqueue(action);
        }

        public virtual async Task SaveChangesAsync()
        {
            try
            {
                await DbContext.SaveChangesAsync();

                while(AfterSaveChangesActions.Count > 0)
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
            return (await DbContext.Database.ExecuteSqlCommandAsync(script)).ToString();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
