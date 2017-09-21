using Microsoft.AspNetCore.Mvc;
using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
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
		new protected TCollection _collection;
		protected IStorageService _storage;

		public WebApiController(TCollection collection, IStorageService storage, IExecutionContext execution, IEntitySerializer serializer, ApiHelper<TEntity, TKey> apiHelper)
			: base(collection, execution, serializer, apiHelper)
		{
			_collection = collection;
			_storage = storage;
		}

		public async virtual Task<IActionResult> PostAsync()
		{
			var query = _apiHelper.CreateQuery(HttpVerb.POST, false);
			var datas = _apiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext).SingleOrDefault();

			var entity = await _collection.CreateAsync(datas, query);

			_execution.queryWatch.Start();

			await _storage.SaveChangesAsync();

			_execution.queryWatch.Stop();

			entity = await _collection.GetEntityAfterCreateAsync(entity, query);

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		public async virtual Task<IActionResult> PutAsync(TKey _id_)
		{
			var query = _apiHelper.CreateQuery(HttpVerb.PUT, false);
			var datas = _apiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext).SingleOrDefault();

			_execution.queryWatch.Start();

			var entity = await _collection.UpdateAsync(_id_, datas, query);

			await _storage.SaveChangesAsync();

			_execution.queryWatch.Start();

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		public async virtual Task<IActionResult> PutAsync()
		{
			var query = _apiHelper.CreateQuery(HttpVerb.PUT, false);
			var datas = _apiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext);

			//Datas est censé contenir un tableau d'objet ayant une prop "id" qui permet de les identifier individuellement
			if (datas.Any(d => !d.ContainsKey("id")))
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest, "PUT on collection implies that you provide an array of objets each of which with an id attribute");
			}

			//Il faut que les id soient convertibles en TKey
			try { var result = datas.Select(d => TypeExtensions.Convert<TKey>(d["id"].value)); }
			catch { throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("PUT on collection implies that each id be of type : {0}", typeof(TKey).Name)); }

			var entities = new HashSet<TEntity>();

			_execution.queryWatch.Start();

			foreach (var d in datas)
			{
				var id = (TKey)TypeExtensions.Convert<TKey>(d["id"].value);

				var entity = await _collection.UpdateAsync(id, d, query);

				entities.Add(entity);
			}

			await _storage.SaveChangesAsync();

			_execution.queryWatch.Stop();

			var dataContainer = new Metadata(_serializer.SerializeEntities(entities, query.Fields), query.Options, query.Page, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		public async virtual Task<IActionResult> DeleteAsync(TKey _id_)
		{
			_execution.queryWatch.Start();

			await _collection.DeleteAsync(_id_);

			await _storage.SaveChangesAsync();

			_execution.queryWatch.Stop();

			return Ok();
		}

		public async virtual Task<IActionResult> DeleteAsync()
		{
			var query = _apiHelper.CreateQuery(HttpVerb.DELETE, true);

			_execution.queryWatch.Start();

			var datas = _apiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext);

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