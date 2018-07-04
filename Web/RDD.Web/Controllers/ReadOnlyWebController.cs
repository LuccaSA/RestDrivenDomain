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

        public virtual async Task<ActionResult<IEnumerable<TEntity>>> GetAsync()
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
                return NotFound();

            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get);

            IEnumerable<TEntity> result = await AppController.GetAsync(query);

            // todo : injecter le Count de ISelection dans le Query ?
            HttpContext.SetContextualQuery(query);

            return Ok(result);
        }

        public virtual async Task<ActionResult<TEntity>> GetByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
                return NotFound();

            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get, false);

            TEntity entity = await AppController.GetByIdAsync(id, query);

            HttpContext.SetContextualQuery(query);

            return entity;
        }
    }
}