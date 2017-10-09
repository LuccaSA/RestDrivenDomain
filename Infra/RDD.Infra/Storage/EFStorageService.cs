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
		protected DbContext _dbContext { get; set; }
		protected Queue<Task> _afterSaveChangesActions { get; set; }

		public EFStorageService(DbContext dbContext)
		{
			_dbContext = dbContext;
			_afterSaveChangesActions = new Queue<Task>();
		}

		public virtual IQueryable<TEntity> Set<TEntity>()
			where TEntity : class, IEntityBase
		{
			return _dbContext.Set<TEntity>();
		}

		public virtual void Add<TEntity>(TEntity entity)
			where TEntity : class, IEntityBase
		{
			_dbContext.Set<TEntity>().Add(entity);
		}

		public virtual void AddRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class, IEntityBase
		{
			_dbContext.Set<TEntity>().AddRange(entities);
		}

		public virtual void Remove<TEntity>(TEntity entity)
			where TEntity : class, IEntityBase
		{
			_dbContext.Set<TEntity>().Remove(entity);
		}

		public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class
		{
			_dbContext.Set<TEntity>().RemoveRange(entities);
		}

		public void AddAfterSaveChangesAction(Task action)
		{
			_afterSaveChangesActions.Enqueue(action);
		}

		public virtual async Task SaveChangesAsync()
		{
			try
			{
				await _dbContext.SaveChangesAsync();

				while(_afterSaveChangesActions.Count > 0)
				{
					await _afterSaveChangesActions.Dequeue();
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
			return (await _dbContext.Database.ExecuteSqlCommandAsync(script)).ToString();
		}
		public void Dispose()
		{
			_dbContext.Dispose();
		}
	}
}
