using RDD.Domain;
using RDD.Domain.Models.Collections;
using RDD.Web.Helpers;

namespace RDD.Web.Controllers
{
	public partial class WebApiController<TCollection, TEntity, TKey> : ReadOnlyWebApiController<TCollection, TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TKey>
	{
		public WebApiController(IExecutionContext execution, TCollection collection, IContext context, IEntitySerializer serializer, IApiHelper<TEntity> apiHelper)
			: base(execution, collection, context, serializer, apiHelper) { }
	}
}