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
	public abstract partial class ReadOnlyWebApiController<TCollection, TEntity, TKey> : ControllerBase
		where TCollection : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public async virtual Task<IActionResult> GetAsync()
		{
			using (var storage = GetStorage())
			{
				var query = _apiHelper.CreateQuery(HttpVerb.GET);
				var collection = GetReadOnlyCollection(storage, GetRepository(storage));

				_execution.queryWatch.Start();

				var selection = await collection.GetAsync(query);

				_execution.queryWatch.Stop();

				var dataContainer = new Metadata(_serializer.SerializeSelection(selection, query), query.Options, query.Page, _execution);

				return Ok(dataContainer.ToDictionary());
			}
		}

		// Attention ! Ne pas renommer _id_ en id, sinon, il est impossible de faire des filtres API sur id dans la querystring
		// car asp.net essaye de mapper vers la TKey id et n'est pas content car c'est pas du bon type
		public async virtual Task<IActionResult> GetAsync(TKey _id_)
		{
			using (var storage = GetStorage())
			{
				var query = _apiHelper.CreateQuery(HttpVerb.GET, false);
				var collection = GetReadOnlyCollection(storage, GetRepository(storage));

				_execution.queryWatch.Start();

				var entity = await collection.GetByIdAsync(_id_, query);

				_execution.queryWatch.Stop();

				var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _execution);

				return Ok(dataContainer.ToDictionary());
			}
		}
	}
}
