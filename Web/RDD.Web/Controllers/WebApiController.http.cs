using Microsoft.AspNetCore.Mvc;
using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Infra;
using RDD.Infra.Contexts;
using RDD.Web.Contexts;
using RDD.Web.Exceptions;
using RDD.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace RDD.Web.Controllers
{
	[JsonException]
	public partial class WebApiController<TCollection, TEntity, TKey> : ReadOnlyWebApiController<TCollection, TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public virtual HttpResponseMessage Post()
		{
			return Post(new HttpRequestMessageWrapper(Request));
		}

		[NonAction]
		public virtual HttpResponseMessage Post(IRequestMessage request)
		{
			var query = ApiHelper.CreateQuery(HttpVerb.POST, false);
			var datas = ApiHelper.InputObjectsFromIncomingHTTPRequest(request).SingleOrDefault();

			var entity = _collection.Create(datas, query);

			_execution.queryWatch.Start();

			_storage.Commit();

			_execution.queryWatch.Stop();

			entity = _collection.GetEntityAfterCreate(entity, query);

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options);

			return request.CreateResponse(HttpStatusCode.OK, dataContainer.ToDictionary(), ApiHelper.GetFormatter());
		}

		public virtual HttpResponseMessage Put(TKey _id_)
		{
			return Put(_id_, new HttpRequestMessageWrapper(Request));
		}

		[NonAction]
		public virtual HttpResponseMessage Put(TKey _id_, IRequestMessage request)
		{
			var query = ApiHelper.CreateQuery(HttpVerb.PUT, false);

			var datas = ApiHelper.InputObjectsFromIncomingHTTPRequest(request).SingleOrDefault();

			_execution.queryWatch.Start();

			var entity = _collection.Update(_id_, datas, query);

			_storage.Commit();

			_execution.queryWatch.Start();

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options);

			return Request.CreateResponse(HttpStatusCode.OK, dataContainer.ToDictionary(), ApiHelper.GetFormatter());
		}

		public virtual HttpResponseMessage Put()
		{
			return Put(new HttpRequestMessageWrapper(Request));
		}

		[NonAction]
		public virtual HttpResponseMessage Put(IRequestMessage request)
		{
			var query = ApiHelper.CreateQuery(HttpVerb.PUT, false);

			var datas = ApiHelper.InputObjectsFromIncomingHTTPRequest(request);

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

			var dataContainer = new Metadata(_serializer.SerializeEntities(entities, query.Fields), query.Options);

			return Request.CreateResponse(HttpStatusCode.OK, dataContainer.ToDictionary(), ApiHelper.GetFormatter());
		}

		public virtual HttpResponseMessage Delete(TKey _id_)
		{
			_execution.queryWatch.Start();

			_collection.Delete(_id_);

			_storage.Commit();

			_execution.queryWatch.Stop();

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		public virtual HttpResponseMessage Delete()
		{
			return Delete(new HttpRequestMessageWrapper(Request));
		}

		[NonAction]
		public virtual HttpResponseMessage Delete(IRequestMessage request)
		{
			var query = ApiHelper.CreateQuery(HttpVerb.DELETE, true);

			_execution.queryWatch.Start();

			var datas = ApiHelper.InputObjectsFromIncomingHTTPRequest(request);

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

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		protected override void Dispose(bool disposing)
		{
			_storage.Dispose();

			base.Dispose(disposing);
		}
	}
}
