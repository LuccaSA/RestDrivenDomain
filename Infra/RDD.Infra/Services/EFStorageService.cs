using Microsoft.EntityFrameworkCore;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Infra.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Services
{
	public class EFStorageService : IStorageService
	{
		protected DbContext _dbContext { get; set; }
		protected ISet<Task> _afterCommitActions { get; set; }

		public EFStorageService(DbContext dbContext)
		{
			_dbContext = dbContext;
			_afterCommitActions = new HashSet<Task>();
		}

		public virtual IQueryable<TEntity> Set<TEntity>()
			where TEntity : class
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

		public virtual TEntity Add<TEntity>(TEntity entity)
			where TEntity : class, IPrimaryKey
		{
			return _dbContext.Set<TEntity>().Add(entity).Entity;
		}

		public virtual void Remove<TEntity>(TEntity entity)
			where TEntity : class
		{
			_dbContext.Set<TEntity>().Remove(entity);
		}

		public void AddRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class, IPrimaryKey
		{
			//http://stackoverflow.com/questions/4355474/how-do-i-speed-up-dbset-add
			try
			{
				_dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
				_dbContext.Set<TEntity>().AddRange(entities);
			}
			finally
			{
				_dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
			}
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

		public void AddAfterCommitAction(Task action)
		{
			_afterCommitActions.Add(action);
		}

		public async virtual Task CommitAsync()
		{
			try
			{
				await _dbContext.SaveChangesAsync();

				foreach (var action in _afterCommitActions)
				{
					await action;
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
