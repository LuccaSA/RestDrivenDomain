using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using RDD.Domain;
using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Storage.MongoDB
{
	public class MongoDBStorageService : IStorageService
	{
		protected MongoDatabase _dbContext { get; set; }
		protected Dictionary<Type, string> _typeToCollectionMapping { get; set; }

		public MongoDBStorageService() { }
		public MongoDBStorageService(string connectionString, string databaseName, Dictionary<Type, string> typeToCollectionMapping)
		{
			var client = new MongoClient(connectionString);
			var server = client.GetServer();

			_dbContext = server.GetDatabase(databaseName);
			_typeToCollectionMapping = typeToCollectionMapping;
		}

		public virtual IQueryable<TEntity> Set<TEntity>()
			where TEntity : class
		{
			return _dbContext.GetCollection<TEntity>(_typeToCollectionMapping[typeof(TEntity)]).AsQueryable();
		}

		public IQueryable<TEntity> Includes<TEntity>(IQueryable<TEntity> entities, PropertySelector<TEntity> includes)
			where TEntity : class
		{
			return entities;
		}

		public virtual TEntity Add<TEntity>(TEntity entity)
			where TEntity : class, IPrimaryKey
		{
			var collection = _dbContext.GetCollection<TEntity>(_typeToCollectionMapping[typeof(TEntity)]);
			
			WriteConcernResult result = collection.Save(entity);

			if (result.HasLastErrorMessage)
			{
				throw new Exception(String.Format("Error while saving: {0}", result.LastErrorMessage));
			}

			// nouvelle resource tout juste créée, on la récupère de la BD
			if (((IMongoDBResource)entity).ID == null)
			{
				var query = Query.EQ("ID", result.Upserted);

				return collection.FindOne(query);
			}

			// resource updatée en BD, déjà à jour
			return entity;
		}

		public virtual void Remove<TEntity>(TEntity entity)
			where TEntity : class
		{
			var query = Query<TEntity>.EQ(e => ((IMongoDBResource)e).ID, ((IMongoDBResource)entity).ID);
			var collection = _dbContext.GetCollection<TEntity>(_typeToCollectionMapping[typeof(TEntity)]);

			WriteConcernResult result = collection.Remove(query);

			if (result.HasLastErrorMessage)
			{
				throw new Exception(String.Format("Error while deleting: {0}", result.LastErrorMessage));
			}
		}

		public void AddRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class, IPrimaryKey { throw new NotImplementedException(); }

		public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class { throw new NotImplementedException(); }

		public void AddAfterCommitAction(Action action)
		{
			throw new NotImplementedException();
		}
		public virtual void Commit() { }
		public void Dispose() { }
	}
}
