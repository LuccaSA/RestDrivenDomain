using Microsoft.EntityFrameworkCore;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Infra.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra.Services
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

		public IQueryable<TEntity> Includes<TEntity>(IQueryable<TEntity> entities, PropertySelector<TEntity> includes)
			where TEntity : class
		{
			foreach (var path in includes.ExtractPaths())
			{
				entities = entities.Include(path);
			}
			return entities;
		}

		public virtual void Add<TEntity>(TEntity entity)
			where TEntity : class, IEntityBase
		{
			_dbContext.Set<TEntity>().Add(entity);
		}

		public virtual void AddRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class, IEntityBase
		{
			//http://stackoverflow.com/questions/4355474/how-do-i-speed-up-dbset-add
			try
			{
				//_dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
				_dbContext.Set<TEntity>().AddRange(entities);
			}
			finally
			{
				_dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
			}
		}

		public virtual void Remove<TEntity>(TEntity entity)
			where TEntity : class, IEntityBase
		{
			_dbContext.Set<TEntity>().Remove(entity);
		}

		public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class
		{

			try
			{
				_dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
				_dbContext.Set<TEntity>().RemoveRange(entities);
			}
			finally
			{
				_dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
			}
		}

		public void AddAfterSaveChangesAction(Task action)
		{
			_afterSaveChangesActions.Enqueue(action);
		}

		public async virtual Task SaveChangesAsync()
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
