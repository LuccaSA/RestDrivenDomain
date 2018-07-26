using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using RDD.Web.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Web.Controllers
{
    public abstract class ReadOnlyWebController<TEntity, TKey> : ReadOnlyWebController<IReadOnlyAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected ReadOnlyWebController(IReadOnlyAppController<TEntity, TKey> appController, ApiHelper<TEntity, TKey> helper)
            : base(appController, helper)
        {
        }
    }

    [Produces(MetaSelectiveJsonOutputFormatter.MetaDataContentType, SelectiveJsonOutputFormatter.SelectiveContentType)]
    public abstract class ReadOnlyWebController<TAppController, TEntity, TKey> : ControllerBase
        where TAppController : IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected TAppController AppController { get; }
        protected ApiHelper<TEntity, TKey> Helper { get; }

        protected ReadOnlyWebController(TAppController appController, ApiHelper<TEntity, TKey> helper)
        {
            AppController = appController;
            Helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        protected virtual HttpVerbs AllowedHttpVerbs => HttpVerbs.None;

        public Task<ActionResult<IEnumerable<TEntity>>> GetAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return ProtectedGetAsync();
            }
            return Task.FromResult((ActionResult<IEnumerable<TEntity>>)NotFound());
        }

        public Task<ActionResult<TEntity>> GetByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return ProtectedGetAsync(id);
            }
            return Task.FromResult((ActionResult<TEntity>)NotFound());
        }

        protected virtual async Task<ActionResult<IEnumerable<TEntity>>> ProtectedGetAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get);

            IEnumerable<TEntity> entity = await AppController.GetAsync(query);

            return Ok(entity);
        }

        protected virtual async Task<ActionResult<TEntity>> ProtectedGetAsync(TKey id)
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get, false);

            TEntity entity = await AppController.GetByIdAsync(id, query);
            if (entity == null)
            {
                return NotFound(id);
            }
            return Ok(entity);
        }

        protected NotFoundObjectResult NotFound(TKey id) => NotFound(new { Id = id, error = $"Resource {id} not found" });
    }
}
