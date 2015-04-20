using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDD.Infra.Models.Enums;
using System.Linq.Expressions;
using RDD.Infra.Models.Querying;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Infra
{
	public interface IRestService
	{
		/// <summary>
		/// Used from reflection
		/// </summary>
		/// <param name="id"></param>
		/// <param name="verb"></param>
		/// <returns></returns>
		object TryGetById(object id, HttpVerb verb = HttpVerb.GET);
	}

	public interface IRestService<TEntity, TKey> : IRestService
		where TEntity : IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		TEntity GetById(TKey id, HttpVerb verb = HttpVerb.GET);
		TEntity GetById(TKey id, Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
		ICollection<TEntity> GetByIds(ISet<TKey> ids, HttpVerb verb = HttpVerb.GET);
		ICollection<TEntity> GetByIds(ISet<TKey> ids, Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
		List<TEntity> GetAll();
		IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, HttpVerb verb = HttpVerb.GET);
		RestCollection<TEntity, TKey> Get(Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
		TEntity Create(object datas);
		TEntity Create(PostedData datas);
		TEntity Update(TKey id, PostedData datas);
		TEntity Update(TKey id, object datas);
		void Delete(TKey id);
	}
}
