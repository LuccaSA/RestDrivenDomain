using Microsoft.EntityFrameworkCore;
using RDD.Infra.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace RDD.Infra.Services
{
	public class EFStorageService : IStorageService
	{
		protected DbContext _dbContext { get; set; }

		public EFStorageService(DbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public virtual IQueryable<TEntity> Set<TEntity>()
			where TEntity : class
		{
			return _dbContext.Set<TEntity>();
		}

		public virtual TEntity Add<TEntity>(TEntity entity)
			where TEntity : class
		{
			return _dbContext.Set<TEntity>().Add(entity).Entity;
		}

		public virtual void Remove<TEntity>(TEntity entity)
			where TEntity : class
		{
			_dbContext.Set<TEntity>().Remove(entity);
		}

		public void AddRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class
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

		public virtual void Commit()
		{
			try
			{
				_dbContext.SaveChanges();
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

		public string ExecuteScript(string script)
		{
			return _dbContext.Database.ExecuteSqlCommand(script).ToString();
		}
		public void Dispose()
		{
			_dbContext.Dispose();
		}
	}
}
