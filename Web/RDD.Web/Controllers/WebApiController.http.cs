using Microsoft.AspNetCore.Mvc;
using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RDD.Web.Controllers
{
	public abstract partial class WebApiController<TCollection, TEntity, TKey> : ReadOnlyWebApiController<TCollection, TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public async virtual Task<IActionResult> PostAsync()
		{
			using (var storage = GetStorage())
			{
				var query = _apiHelper.CreateQuery(HttpVerb.POST, false);
				var datas = _apiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext).SingleOrDefault();
				var collection = GetCollection(storage, GetRepository(storage));

				var entity = await collection.CreateAsync(datas, query);

				_execution.queryWatch.Start();

				await storage.SaveChangesAsync();

				_execution.queryWatch.Stop();

				entity = await collection.GetEntityAfterCreateAsync(entity, query);

				var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _execution);

				return Ok(dataContainer.ToDictionary());
			}
		}

		public async virtual Task<IActionResult> PutAsync(TKey _id_)
		{
			using (var storage = GetStorage())
			{
				var query = _apiHelper.CreateQuery(HttpVerb.PUT, false);
				var datas = _apiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext).SingleOrDefault();
				var collection = GetCollection(storage, GetRepository(storage));

				_execution.queryWatch.Start();

				var entity = await collection.UpdateAsync(_id_, datas, query);

				await storage.SaveChangesAsync();

				_execution.queryWatch.Start();

				var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _execution);

				return Ok(dataContainer.ToDictionary());
			}
		}

		public async virtual Task<IActionResult> PutAsync()
		{
			using (var storage = GetStorage())
			{
				var query = _apiHelper.CreateQuery(HttpVerb.PUT, false);
				var datas = _apiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext);
				var collection = GetCollection(storage, GetRepository(storage));

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

					var entity = await collection.UpdateAsync(id, d, query);

					entities.Add(entity);
				}

				await storage.SaveChangesAsync();

				_execution.queryWatch.Stop();

				var dataContainer = new Metadata(_serializer.SerializeEntities(entities, query.Fields), query.Options, query.Page, _execution);

				return Ok(dataContainer.ToDictionary());
			}
		}

		public async virtual Task<IActionResult> DeleteAsync(TKey _id_)
		{
			using (var storage = GetStorage())
			{
				var collection = GetCollection(storage, GetRepository(storage));

				_execution.queryWatch.Start();

				await collection.DeleteAsync(_id_);

				await storage.SaveChangesAsync();

				_execution.queryWatch.Stop();

				return Ok();
			}
		}

		public async virtual Task<IActionResult> DeleteAsync()
		{
			using (var storage = GetStorage())
			{
				var query = _apiHelper.CreateQuery(HttpVerb.DELETE, true);
				var collection = GetCollection(storage, GetRepository(storage));

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

					await collection.DeleteAsync(id);
				}

				await storage.SaveChangesAsync();

				return Ok();
			}
		}
	}
}
