using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Web.Helpers;
using RDD.Web.Models;
using System;
using System.Threading.Tasks;

namespace RDD.Web.Controllers
{
	public abstract class ReadOnlyWebController<TAppController, TEntity, TKey> : ControllerBase
		where TAppController : IReadOnlyAppController<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected TAppController _appController;
		protected ApiHelper<TEntity, TKey> _helper;

		public ReadOnlyWebController(TAppController appController, ApiHelper<TEntity, TKey> helper)
		{
			_appController = appController;
			_helper = helper;
		}

		protected async virtual Task<IActionResult> ProtectedGetAsync()
		{
			_helper.WebContextWrapper.SetContext(HttpContext);
			var query = _helper.CreateQuery(HttpVerb.GET);

			var selection = await _appController.GetAsync(query);

			var dataContainer = new Metadata(_helper.Serializer.SerializeSelection(selection, query), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}

		// Attention ! Ne pas renommer _id_ en id, sinon, il est impossible de faire des filtres API sur id dans la querystring
		// car asp.net essaye de mapper vers la TKey id et n'est pas content car c'est pas du bon type
		protected async virtual Task<IActionResult> ProtectedGetAsync(TKey _id_)
		{
			_helper.WebContextWrapper.SetContext(HttpContext);
			var query = _helper.CreateQuery(HttpVerb.GET, false);

			var entity = await _appController.GetByIdAsync(_id_, query);

			var dataContainer = new Metadata(_helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _helper.Execution);

			return Ok(dataContainer.ToDictionary());
		}
	}
}