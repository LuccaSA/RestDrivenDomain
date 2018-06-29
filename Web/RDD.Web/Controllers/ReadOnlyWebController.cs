using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Web.Helpers;
using RDD.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Controllers
{
    public abstract class ReadOnlyWebController<TEntity, TKey> : ReadOnlyWebController<IReadOnlyAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected ReadOnlyWebController(IReadOnlyAppController<TEntity, TKey> appController, ApiHelper<TEntity, TKey> helper) 
            : base(appController, helper)
        {
        }
    }

    public abstract class ReadOnlyWebController<TAppController, TEntity, TKey> : ControllerBase
        where TAppController : IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
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

        public async Task<ActionResult<IEnumerable<TEntity>>> GetAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return await ProtectedGetAsync();
            }
            return NotFound();
        }

        public async Task<ActionResult<TEntity>> GetByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return await ProtectedGetAsync(id);
            }
            return NotFound();
        }
        
        protected virtual async Task<ActionResult<IEnumerable<TEntity>>> ProtectedGetAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get);

            IEnumerable<TEntity> result = await AppController.GetAsync(query);

            // todo : injecter le Count de ISelection dans le Query ?
            HttpContext.SetContextualQuery(query);
            
            return Ok(result);
        }

        // Attention ! Ne pas renommer _id_ en id, sinon, il est impossible de faire des filtres API sur id dans la querystring
        // car asp.net essaye de mapper vers la TKey id et n'est pas content car c'est pas du bon type
        protected virtual async Task<ActionResult<TEntity>> ProtectedGetAsync(TKey id)
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get, false);

            TEntity entity = await AppController.GetByIdAsync(id, query);

            HttpContext.SetContextualQuery(query);

            return entity;
        }
    }
}