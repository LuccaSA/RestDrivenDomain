using LinqKit;
using Newtonsoft.Json;
using RDD.Domain.Helpers;
using RDD.Infra;
using RDD.Infra.Helpers;
using RDD.Infra.Models.Enums;
using RDD.Infra.Models.Exceptions;
using RDD.Infra.Models.Querying;
using RDD.Infra.Models.Rights;
using RDD.Infra.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Services
{
	public partial class RestService<TEntity, IEntity, TKey> : IRestService<TEntity, IEntity, TKey>
		where TEntity : class, IEntity, new()
		where IEntity : IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		public IEntity Create(object datas)
		{
			return Create(PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		}
		public IEntity Create(PostedData datas)
		{
			var entity = InstanciateEntity();

			//			GetPatcher(RepoProvider.Current, _context).PatchEntity(entity, datas);

			CheckRightsForCreate(entity);

			Create(entity);

			return entity;
		}
		public IEntity Update(TKey id, object datas)
		{
			return Update(id, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		}
		public IEntity Update(TKey id, PostedData datas)
		{
			var entity = GetEntityById(id, HttpVerb.PUT);

			return UpdateEntity(entity, datas);
		}
		public void Delete(TKey id)
		{
			//var entity = GetById(id, HttpVerb.DELETE);

			//AttachOperationsToEntity(entity);
			//AttachActionsToEntity(entity);

			//Delete(entity);
		}
		public IEnumerable<IEntity> Get(Expression<Func<IEntity, bool>> filter, HttpVerb verb)
		{
			return Get(new Query<IEntity> { ExpressionFilters = filter }, verb).Items;
		}
		public List<IEntity> GetAll()
		{
			return Get(new Query<IEntity>(), HttpVerb.GET).Items.ToList();
		}
		public virtual RestCollection<IEntity, TKey> Get(Query<IEntity> query, HttpVerb verb)
		{
			var collection = GetEntities(query, verb);

			var result = new RestCollection<IEntity, TKey>();
			result.Count = collection.Count;
			result.Items = collection.Items.Select(i => (IEntity)i).ToList();

			return result;
		}
		public IEntity GetById(TKey id, HttpVerb verb)
		{
			return GetEntityById(id, new Query<IEntity>(), verb);
		}
		public virtual IEntity GetById(TKey id, Query<IEntity> query, HttpVerb verb)
		{
			return GetEntityById(id, query, verb);
		}

		public ICollection<IEntity> GetByIds(ISet<TKey> ids, HttpVerb verb)
		{
			return GetByIds(ids, new Query<IEntity>(), verb);
		}
		public virtual ICollection<IEntity> GetByIds(ISet<TKey> ids, Query<IEntity> query, HttpVerb verb)
		{
			return GetEntitiesByIds(ids, query, verb).Cast<IEntity>().ToList();
		}
	}
}
