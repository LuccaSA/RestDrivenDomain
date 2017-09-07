using Microsoft.AspNetCore.Mvc;
using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Collections;
using RDD.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RDD.Web.Controllers
{
	public partial class WebApiController<TCollection, TEntity, TKey> : ReadOnlyWebApiController<TCollection, TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TKey>
	{
		[NonAction]
		public virtual IActionResult Post()
		{
			var query = _apiHelper.CreateQuery(false);
			var datas = _apiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext).SingleOrDefault();

			var entity = _collection.Create(datas, query);
			
			_context.SaveChanges();

			entity = _collection.GetEntityAfterCreate(entity, query);

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		[NonAction]
		public virtual IActionResult Put(TKey _id_)
		{
			var query = _apiHelper.CreateQuery(false);

			var datas = _apiHelper.InputObjectsFromIncomingHTTPRequest(HttpContext).SingleOrDefault();
			
			var entity = _collection.Update(_id_, datas, query);

			_context.SaveChanges();

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		[NonAction]
		public virtual IActionResult Put()
		{
			var query = _apiHelper.CreateQuery(false);

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
			
			foreach (var d in datas)
			{
				var id = (TKey)TypeExtensions.Convert<TKey>(d["id"].value);

				var entity = _collection.Update(id, d, query);

				entities.Add(entity);
			}

			_context.SaveChanges();

			var dataContainer = new Metadata(_serializer.SerializeEntities(entities, query.Fields), query.Options, _execution);

			return Ok(dataContainer.ToDictionary());
		}

		public virtual IActionResult Delete(TKey _id_)
		{
			_collection.Delete(_id_);

			_context.SaveChanges();

			return Ok();
		}

		[NonAction]
		public virtual IActionResult Delete()
		{
			var query = _apiHelper.CreateQuery(true);

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

				_collection.Delete(id);
			}

			_context.SaveChanges();

			return Ok();
		}
	}
}
