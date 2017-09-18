using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Infra;
using System;

namespace RDD.Web.Controllers
{
	public abstract partial class WebApiController<TCollection, TEntity, TKey> : ReadOnlyWebApiController<TCollection, TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public WebApiController(IExecutionContext execution, IEntitySerializer serializer, ICombinationsHolder combinationHolder, IWebContext webContext, IContractResolver jsonResolver = null)
			: base(execution, serializer, combinationHolder, webContext, jsonResolver) { }
	}
}