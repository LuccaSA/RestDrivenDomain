﻿using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Infra.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Services
{
	public class EFStorageService : IStorageService
	{
		protected DbContext _dbContext { get; set; }

		public EFStorageService() { }
		public EFStorageService(DbContext dbContext)
		{
			_dbContext = dbContext;
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

		public virtual void Add<TEntity>(TEntity entity)
			where TEntity : class
		{
			_dbContext.Set<TEntity>().Add(entity);
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
				_dbContext.Configuration.AutoDetectChangesEnabled = false;
				_dbContext.Set<TEntity>().AddRange(entities);
			}
			finally
			{
				_dbContext.Configuration.AutoDetectChangesEnabled = true;
			}
		}

		public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class
		{

			try
			{
				_dbContext.Configuration.AutoDetectChangesEnabled = false;
				_dbContext.Set<TEntity>().RemoveRange(entities);
			}
			finally
			{
				_dbContext.Configuration.AutoDetectChangesEnabled = true;
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
				UpdateException updateException = (UpdateException)ex.InnerException;

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
			catch (DbEntityValidationException ex)
			{
				// Retrieve the error messages as a list of strings.
				var errorMessages = ex.EntityValidationErrors
						.SelectMany(x => x.ValidationErrors)
						.Select(x => String.Format("Property : {0}, ErrorMessage: {1}", x.PropertyName, x.ErrorMessage));

				// Join the list to a single string.
				var fullErrorMessage = string.Join("; ", errorMessages);

				// Combine the original exception message with the new one.
				var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

				// Throw a new DbEntityValidationException with the improved exception message.
				throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
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