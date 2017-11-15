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
    public abstract class ReadOnlyWebController<TEntity, TKey> : ReadOnlyWebController<IReadOnlyAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected ReadOnlyWebController(IReadOnlyAppController<TEntity, TKey> appController, ApiHelper<TEntity, TKey> helper) 
            : base(appController, helper)
        {
        }
    }

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

        protected virtual HttpVerbs AllowedHttpVerbs => HttpVerbs.None;

        public Task<IActionResult> GetAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return ProtectedGetAsync();
            }
            return Task.FromResult((IActionResult)NotFound());
        }

        public Task<IActionResult> GetByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return ProtectedGetAsync(id);
            }
            return Task.FromResult((IActionResult)NotFound());
        }
        
        protected virtual async Task<IActionResult> ProtectedGetAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get);

            ISelection<TEntity> selection = await AppController.GetAsync(query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeSelection(selection, query), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        // Attention ! Ne pas renommer _id_ en id, sinon, il est impossible de faire des filtres API sur id dans la querystring
        // car asp.net essaye de mapper vers la TKey id et n'est pas content car c'est pas du bon type
        protected virtual async Task<IActionResult> ProtectedGetAsync(TKey id)
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get, false);

            TEntity entity = await AppController.GetByIdAsync(id, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }
    }
}