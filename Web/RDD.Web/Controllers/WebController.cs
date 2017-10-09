using Microsoft.AspNetCore.Mvc;
using NExtends.Primitives.Types;
using RDD.Application;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using RDD.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RDD.Web.Controllers
{
	public abstract class WebController<TAppController, TEntity, TKey> : ReadOnlyWebController<TAppController, TEntity, TKey>
		where TAppController : IAppController<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public WebController(TAppController appController, ApiHelper<TEntity, TKey> helper)
			: base(appController, helper) { }

		protected async virtual Task<IActionResult> ProtectedPostAsync()
		{
			_helper.WebContextWrapper.SetContext(HttpContext);
			var query = _helper.CreateQuery(HttpVerb.POST, false);
			var datas = _helper.InputObjectsFromIncomingHTTPRequest().SingleOrDefault();

			var entity = await _appController.CreateAsync(datas, query);

			var dataContainer = new Metadata(_helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}

		protected async virtual Task<IActionResult> ProtectedPutAsync(TKey _id_)
		{
			_helper.WebContextWrapper.SetContext(HttpContext);

			var query = _helper.CreateQuery(HttpVerb.PUT, false);
			var datas = _helper.InputObjectsFromIncomingHTTPRequest().SingleOrDefault();

			var entity = await _appController.UpdateByIdAsync(_id_, datas, query);

			var dataContainer = new Metadata(_helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}

		protected async virtual Task<IActionResult> ProtectedPutAsync()
		{
			_helper.WebContextWrapper.SetContext(HttpContext);
			var query = _helper.CreateQuery(HttpVerb.PUT, false);
			var datas = _helper.InputObjectsFromIncomingHTTPRequest();

			//Datas est censé contenir un tableau d'objet ayant une prop "id" qui permet de les identifier individuellement
			if (datas.Any(d => !d.ContainsKey("id")))
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest, "PUT on collection implies that you provide an array of objets each of which with an id attribute");
			}

			Dictionary<TKey, PostedData> datasByIds;

			//Il faut que les id soient convertibles en TKey
			try
			{
				datasByIds = datas.ToDictionary(el => (TKey)TypeExtensions.Convert<TKey>(el["id"].value), el => el);
			}
			catch
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("PUT on collection implies that each id be of type : {0}", typeof(TKey).Name));
			}

			var entities = await _appController.UpdateByIdsAsync(datasByIds, query);

			var dataContainer = new Metadata(_helper.Serializer.SerializeEntities(entities, query.Fields), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}

		protected async virtual Task<IActionResult> ProtectedDeleteAsync(TKey _id_)
		{
			_helper.WebContextWrapper.SetContext(HttpContext);

			await _appController.DeleteByIdAsync(_id_);

			return Ok();
		}

		protected async virtual Task<IActionResult> ProtectedDeleteAsync()
		{
			_helper.WebContextWrapper.SetContext(HttpContext);
			var query = _helper.CreateQuery(HttpVerb.DELETE, true);
			var datas = _helper.InputObjectsFromIncomingHTTPRequest();

			if (datas.Any(d => !d.ContainsKey("id")))
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest, "DELETE on collection implies that you provide an array of objets each of which with an id attribute");
			}

			IList<TKey> ids;

			//Il faut que les id soient convertibles en TKey
			try
			{
				ids = datas.Select(el => (TKey)TypeExtensions.Convert<TKey>(el["id"].value)).ToList();
			}
			catch
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("DELETE on collection implies that each id be of type : {0}", typeof(TKey).Name));
			}

			await _appController.DeleteByIdsAsync(ids);

			return Ok();
		}
	}
}