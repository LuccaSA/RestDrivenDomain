using Microsoft.AspNetCore.Mvc;
using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Infra.Contexts;
using RDD.Web.Helpers;
using RDD.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RDD.Web.Controllers
{
	public abstract class WebApiController<TCollection, TEntity, TKey> : ReadOnlyWebApiController<TCollection, TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected IStorageService _storage;

		public WebApiController(IStorageService storage, TCollection collection, ApiHelper<TEntity, TKey> helper)
			: base(collection, helper)
		{
			_storage = storage;
		}

		protected virtual Task<IActionResult> ProtectedPostAsync()
		{
			_helper.WebContextWrapper.SetContext(HttpContext);

			return ProtectedPostAsyncAfterContext();
		}

		protected async virtual Task<IActionResult> ProtectedPostAsyncAfterContext()
		{
			var query = _helper.CreateQuery(HttpVerb.POST, false);
			var datas = _helper.InputObjectsFromIncomingHTTPRequest().SingleOrDefault();

			var entity = await _collection.CreateAsync(datas, query);

			_helper.Execution.queryWatch.Start();

			await _storage.SaveChangesAsync();

			_helper.Execution.queryWatch.Stop();

			entity = await _collection.GetEntityAfterCreateAsync(entity, query);

			var dataContainer = new Metadata(_helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}

		protected virtual Task<IActionResult> ProtectedPutAsync(TKey _id_)
		{
			_helper.WebContextWrapper.SetContext(HttpContext);

			return ProtectedPutAsyncAfterContext(_id_);
		}
		protected async virtual Task<IActionResult> ProtectedPutAsyncAfterContext(TKey _id_)
		{
			var query = _helper.CreateQuery(HttpVerb.PUT, false);
			var datas = _helper.InputObjectsFromIncomingHTTPRequest().SingleOrDefault();

			_helper.Execution.queryWatch.Start();

			var entity = await _collection.UpdateAsync(_id_, datas, query);

			await _storage.SaveChangesAsync();

			_helper.Execution.queryWatch.Start();

			var dataContainer = new Metadata(_helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}

		protected virtual Task<IActionResult> ProtectedPutAsync()
		{
			_helper.WebContextWrapper.SetContext(HttpContext);

			return ProtectedPutAsyncAfterContext();
		}
		protected async virtual Task<IActionResult> ProtectedPutAsyncAfterContext()
		{
			var query = _helper.CreateQuery(HttpVerb.PUT, false);
			var datas = _helper.InputObjectsFromIncomingHTTPRequest();

			//Datas est censé contenir un tableau d'objet ayant une prop "id" qui permet de les identifier individuellement
			if (datas.Any(d => !d.ContainsKey("id")))
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest, "PUT on collection implies that you provide an array of objets each of which with an id attribute");
			}

			//Il faut que les id soient convertibles en TKey
			try { var result = datas.Select(d => TypeExtensions.Convert<TKey>(d["id"].value)); }
			catch { throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("PUT on collection implies that each id be of type : {0}", typeof(TKey).Name)); }

			var entities = new HashSet<TEntity>();

			_helper.Execution.queryWatch.Start();

			foreach (var d in datas)
			{
				var id = (TKey)TypeExtensions.Convert<TKey>(d["id"].value);

				var entity = await _collection.UpdateAsync(id, d, query);

				entities.Add(entity);
			}

			await _storage.SaveChangesAsync();

			_helper.Execution.queryWatch.Stop();

			var dataContainer = new Metadata(_helper.Serializer.SerializeEntities(entities, query.Fields), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}

		protected virtual Task<IActionResult> ProtectedDeleteAsync(TKey _id_)
		{
			_helper.WebContextWrapper.SetContext(HttpContext);

			return ProtectedDeleteAsyncAfterContext(_id_);
		}
		protected async virtual Task<IActionResult> ProtectedDeleteAsyncAfterContext(TKey _id_)
		{
			_helper.Execution.queryWatch.Start();

			await _collection.DeleteAsync(_id_);

			await _storage.SaveChangesAsync();

			_helper.Execution.queryWatch.Stop();

			return Ok();
		}

		protected virtual Task<IActionResult> ProtectedDeleteAsync()
		{
			_helper.WebContextWrapper.SetContext(HttpContext);

			return ProtectedDeleteAsyncAfterContext();
		}
		protected async virtual Task<IActionResult> ProtectedDeleteAsyncAfterContext()
		{
			var query = _helper.CreateQuery(HttpVerb.DELETE, true);

			_helper.Execution.queryWatch.Start();

			var datas = _helper.InputObjectsFromIncomingHTTPRequest();

			if (datas.Any(d => !d.ContainsKey("id")))
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest, "DELETE on collection implies that you provide an array of objets each of which with an id attribute");
			}

			//Il faut que les id soient convertibles en TKey
			try { var result = datas.Select(d => TypeExtensions.Convert<TKey>(d["id"].value)); }
			catch { throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("DELETE on collection implies that each id be of type : {0}", typeof(TKey).Name)); }

			foreach (var d in datas)
			{
				var id = (TKey)TypeExtensions.Convert<TKey>(d["id"].value);

				await _collection.DeleteAsync(id);
			}

			await _storage.SaveChangesAsync();

			return Ok();
		}
	}
}