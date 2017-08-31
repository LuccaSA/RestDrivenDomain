using Microsoft.AspNetCore.Mvc;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Contexts;
using RDD.Web.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RDD.Web.Controllers
{
	public partial class ReadOnlyWebApiController<TCollection, TEntity, TKey> : ApiController
		where TCollection : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public virtual HttpResponseMessage Get()
		{
			return Get(query => _collection.Get(query));
		}

		[NonAction]
		protected virtual HttpResponseMessage Get(Func<Query<TEntity>, ISelection<TEntity>> getEntities)
		{
			var query = ApiHelper.CreateQuery(HttpVerb.GET);

			_execution.queryWatch.Start();

			var selection = getEntities(query);

			_execution.queryWatch.Stop();

			var dataContainer = new Metadata(_serializer.SerializeSelection(selection, query.Fields), query.Options, _execution);

			return Request.CreateResponse(HttpStatusCode.OK, dataContainer.ToDictionary(), ApiHelper.GetFormatter());
		}

		// Attention ! Ne pas renommer _id_ en id, sinon, il est impossible de faire des filtres API sur id dans la querystring
		// car asp.net essaye de mapper vers la TKey id et n'est pas content car c'est pas du bon type
		public virtual HttpResponseMessage Get(TKey _id_)
		{
			return Get(_id_, new HttpRequestMessageWrapper(Request));
		}

		[NonAction]
		public virtual HttpResponseMessage Get(TKey _id_, IRequestMessage request)
		{
			return request.CreateResponse(HttpStatusCode.OK, GetEntity(_id_), ApiHelper.GetFormatter());
		}

		[NonAction]
		public Dictionary<string, object> GetEntity(TKey id)
		{
			var query = ApiHelper.CreateQuery(HttpVerb.GET, false);

			_execution.queryWatch.Start();

			var entity = _collection.GetById(id, query);

			_execution.queryWatch.Stop();

			var dataContainer = new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, _execution);

			return dataContainer.ToDictionary();
		}

		protected override void Dispose(bool disposing)
		{
			_storage.Dispose();

			base.Dispose(disposing);
		}
	}
}
