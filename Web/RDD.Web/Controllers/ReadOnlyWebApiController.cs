using Microsoft.AspNetCore.Mvc;
using RDD.Domain;
using RDD.Domain.Models.Collections;
using RDD.Web.Helpers;

namespace RDD.Web.Controllers
{
	public partial class ReadOnlyWebApiController<TCollection, TEntity, TKey> : ControllerBase
		where TCollection : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TKey>
	{
		protected IExecutionContext _execution;
		protected TCollection _collection;
		protected IContext _context;

		protected IEntitySerializer _serializer;
		protected IApiHelper<TEntity> _apiHelper { get; set; }

		public ReadOnlyWebApiController(IExecutionContext execution, TCollection collection, IContext context, IEntitySerializer serializer, IApiHelper<TEntity> apiHelper)
		{
			_execution = execution;
			_collection = collection;
			_serializer = serializer;
			_context = context;

			_apiHelper = apiHelper;
		}
	}
}