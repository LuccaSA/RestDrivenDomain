using Microsoft.AspNetCore.Mvc;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Web.Helpers;
using RDD.Web.Models;
using System;
using System.Threading.Tasks;

namespace RDD.Web.Controllers
{
	public abstract class ReadOnlyWebApiController<TCollection, TEntity, TKey> : ControllerBase
		where TCollection : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected TCollection _collection;
		protected IExecutionContext _execution;
		protected IEntitySerializer _serializer;
		protected ApiHelper<TEntity, TKey> _apiHelper;

		public ReadOnlyWebApiController(TCollection collection, IExecutionContext execution, IEntitySerializer serializer, ApiHelper<TEntity, TKey> apiHelper)
		{
			_collection = collection;
			_execution = execution;
			_serializer = serializer;
			_apiHelper = apiHelper;
		}

		public async virtual Task<IActionResult> GetAsync()
		{
			var query = _apiHelper.CreateQuery(HttpVerb.GET);

			_execution.queryWatch.Start();

			var selection = await _collection.GetAsync(query);

			_execution.queryWatch.Stop();

			var dataContainer = new Metadata(_serializer.SerializeSelection(selection, query), query.Options, query.Page, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		// Attention ! Ne pas renommer _id_ en id, sinon, il est impossible de faire des filtres API sur id dans la querystring
		// car asp.net essaye de mapper vers la TKey id et n'est pas content car c'est pas du bon type
		public async virtual Task<IActionResult> GetAsync(TKey _id_)
		{
			var query = _apiHelper.CreateQuery(HttpVerb.GET, false);

			_execution.queryWatch.Start();

			var entity = await _collection.GetByIdAsync(_id_, query);

			_execution.queryWatch.Stop();

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _execution);

			return Ok(dataContainer.ToDictionary());
		}
	}
}