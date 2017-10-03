using Microsoft.AspNetCore.Http;
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
		protected ApiHelper<TEntity, TKey> _helper;

		public ReadOnlyWebApiController(TCollection collection, ApiHelper<TEntity, TKey> helper)
		{
			_collection = collection;
			_helper = helper;
		}

		protected virtual Task<IActionResult> ProtectedGetAsync()
		{
			_helper.WebContextWrapper.SetContext(HttpContext);

			return ProtectedGetAsyncAfterContext();
		}

		protected async virtual Task<IActionResult> ProtectedGetAsyncAfterContext()
		{ 
			var query = _helper.CreateQuery(HttpVerb.GET);

			_helper.Execution.queryWatch.Start();

			var selection = await _collection.GetAsync(query);

			_helper.Execution.queryWatch.Stop();

			var dataContainer = new Metadata(_helper.Serializer.SerializeSelection(selection, query), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}

		// Attention ! Ne pas renommer _id_ en id, sinon, il est impossible de faire des filtres API sur id dans la querystring
		// car asp.net essaye de mapper vers la TKey id et n'est pas content car c'est pas du bon type
		protected virtual Task<IActionResult> ProtectedGetAsync(TKey _id_)
		{
			_helper.WebContextWrapper.SetContext(HttpContext);

			return ProtectedGetAsyncAfterContext(_id_);
		}
		protected async virtual Task<IActionResult> ProtectedGetAsyncAfterContext(TKey _id_)
		{
			var query = _helper.CreateQuery(HttpVerb.GET, false);

			_helper.Execution.queryWatch.Start();

			var entity = await _collection.GetByIdAsync(_id_, query);

			_helper.Execution.queryWatch.Stop();

			var dataContainer = new Metadata(_helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}
	}
}