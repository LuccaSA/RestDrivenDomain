using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Web.Helpers;
using RDD.Web.Models;
using System;
using System.Threading.Tasks;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Controllers
{
    public abstract class ReadOnlyWebController<TAppController, TEntity, TKey> : ControllerBase
        where TAppController : IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected TAppController AppController { get; }
        protected ApiHelper<TEntity, TKey> Helper { get; }

        protected ReadOnlyWebController(TAppController appController, ApiHelper<TEntity, TKey> helper)
        {
            AppController = appController;
            Helper = helper;
        }

        protected virtual async Task<IActionResult> ProtectedGetAsync()
        {
            Helper.WebContextWrapper.SetContext(HttpContext);

            Query<TEntity> query = Helper.CreateQuery(HttpVerb.GET);

            ISelection<TEntity> selection = await AppController.GetAsync(query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeSelection(selection, query), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        // Attention ! Ne pas renommer _id_ en id, sinon, il est impossible de faire des filtres API sur id dans la querystring
        // car asp.net essaye de mapper vers la TKey id et n'est pas content car c'est pas du bon type
        protected virtual async Task<IActionResult> ProtectedGetAsync(TKey id)
        {
            Helper.WebContextWrapper.SetContext(HttpContext);
            Query<TEntity> query = Helper.CreateQuery(HttpVerb.GET, false);

            TEntity entity = await AppController.GetByIdAsync(id, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }
    }
}