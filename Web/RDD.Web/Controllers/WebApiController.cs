using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;

namespace RDD.Web.Controllers
{
	public partial class WebApiController<TCollection, TEntity, TKey> : ReadOnlyWebApiController<TCollection, TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public WebApiController(IWebContext webContext, IExecutionContext execution, Func<IStorageService> newStorage, IEntitySerializer serializer, Query<TEntity> query = null, IContractResolver jsonResolver = null)
			: base(webContext, execution, newStorage, serializer, query, jsonResolver) { }
	}
}