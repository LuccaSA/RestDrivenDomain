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

namespace RDD.Web.Controllers
{
	public partial class WebApiController<TCollection, TEntity, TKey> : ReadOnlyWebApiController<TCollection, TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		[NonAction]
		public virtual IActionResult Post()
		{
			var query = ApiHelper.CreateQuery(HttpVerb.POST, false);
			var datas = ApiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext).SingleOrDefault();

			var entity = _collection.Create(datas, query);

			_execution.queryWatch.Start();

			_storage.Commit();

			_execution.queryWatch.Stop();

			entity = _collection.GetEntityAfterCreate(entity, query);

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		[NonAction]
		public virtual IActionResult Put(TKey _id_)
		{
			var query = ApiHelper.CreateQuery(HttpVerb.PUT, false);

			var datas = ApiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext).SingleOrDefault();

			_execution.queryWatch.Start();

			var entity = _collection.Update(_id_, datas, query);

			_storage.Commit();

			_execution.queryWatch.Start();

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, _execution);

			return Ok(dataContainer.ToDictionary());//, ApiHelper.GetFormatter());
		}

		[NonAction]
		public virtual IActionResult Put()
		{
			var query = ApiHelper.CreateQuery(HttpVerb.PUT, false);

			var datas = ApiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext);

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

				var entity = _collection.Update(id, d, query);

				entities.Add(entity);
			}

			_storage.Commit();

			_execution.queryWatch.Stop();

			var dataContainer = new Metadata(_serializer.SerializeEntities(entities, query.Fields), query.Options, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		public virtual IActionResult Delete(TKey _id_)
		{
			_execution.queryWatch.Start();

			_collection.Delete(_id_);

			_storage.Commit();

			_execution.queryWatch.Stop();

			return Ok();
		}

		[NonAction]
		public virtual IActionResult Delete()
		{
			var query = ApiHelper.CreateQuery(HttpVerb.DELETE, true);

			_execution.queryWatch.Start();

			var datas = ApiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext);

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

				_collection.Delete(id);
			}

			_storage.Commit();

			return Ok();
		}
	}
}
