using Microsoft.AspNetCore.Mvc;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Web.Controllers
{
	public partial class ReadOnlyWebApiController<TCollection, TEntity, TKey> : ControllerBase
		where TCollection : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public async virtual Task<IActionResult> GetAsync()
		{
			return await GetAsync(query => _collection.GetAsync(query));
		}

		// Attention ! Ne pas renommer _id_ en id, sinon, il est impossible de faire des filtres API sur id dans la querystring
		// car asp.net essaye de mapper vers la TKey id et n'est pas content car c'est pas du bon type
		public async virtual Task<IActionResult> GetAsync(TKey _id_)
		{
			return Ok(await GetEntityAsync(_id_));
		}

		[NonAction]
		protected async virtual Task<IActionResult> GetAsync(Func<Query<TEntity>, Task<ISelection<TEntity>>> getEntitiesAsync)
		{
			var query = ApiHelper.CreateQuery(HttpVerb.GET);

			_execution.queryWatch.Start();

			var selection = await getEntitiesAsync(query);

			_execution.queryWatch.Stop();

			var dataContainer = new Metadata(_serializer.SerializeSelection(selection, query.Fields), query.Options, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		[NonAction]
		public async Task<Dictionary<string, object>> GetEntityAsync(TKey id)
		{
			var query = ApiHelper.CreateQuery(HttpVerb.GET, false);

			_execution.queryWatch.Start();

			var entity = await _collection.GetByIdAsync(id, query);

			_execution.queryWatch.Stop();

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, _execution);

			return dataContainer.ToDictionary();
		}
	}
}
