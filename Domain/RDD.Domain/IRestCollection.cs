using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IRestCollection<TEntity> : IReadOnlyRestCollection<TEntity>
		where TEntity : class, IEntityBase
	{
		TEntity Create(object datas, Query<TEntity> query = null);
		TEntity Create(PostedData datas, Query<TEntity> query = null);
		void Create(TEntity entity, Query<TEntity> query = null);
		TEntity GetEntityAfterCreate(TEntity entity, Query<TEntity> query = null);
		void CreateRange(IEnumerable<TEntity> entities, Query<TEntity> query = null);

		void Delete(TEntity entity);
		void DeleteRange(IEnumerable<TEntity> entities);
	}

	public interface IRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		TEntity Update(TKey id, PostedData datas, Query<TEntity> query = null);
		TEntity Update(TKey id, object datas, Query<TEntity> query = null);
	
		void Delete(TKey id);
	}
}
