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

	public interface IRestService<IEntity, TKey> : IRestService
		where IEntity : IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		IEntity GetById(TKey id, HttpVerb verb = HttpVerb.GET);
		IEntity GetById(TKey id, Query query, HttpVerb verb = HttpVerb.GET);
		ICollection<IEntity> GetByIds(ISet<TKey> ids, HttpVerb verb = HttpVerb.GET);
		ICollection<IEntity> GetByIds(ISet<TKey> ids, Query query, HttpVerb verb = HttpVerb.GET);
		List<IEntity> GetAll();
		IEnumerable<IEntity> Get(Expression<Func<IEntity, bool>> filter, HttpVerb verb = HttpVerb.GET);
		IEnumerable<IEntity> Get(Expression<Func<IEntity, bool>> filter, Field fields, HttpVerb verb = HttpVerb.GET);
		RestCollection<IEntity, TKey> Get(Query query, HttpVerb verb = HttpVerb.GET);
		IEntity Create(object datas);
		IEntity Create(PostedData datas);
		IEntity Update(TKey id, PostedData datas);
		IEntity Update(TKey id, object datas);
		void Delete(TKey id);
	}
}
