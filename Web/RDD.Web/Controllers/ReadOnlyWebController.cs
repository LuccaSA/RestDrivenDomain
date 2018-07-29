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
using RDD.Web.Querying;

namespace RDD.Web.Controllers
{
    public abstract class ReadOnlyWebController<TEntity, TKey> : ReadOnlyWebController<IReadOnlyAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected ReadOnlyWebController(IReadOnlyAppController<TEntity, TKey> appController, ICandidateFactory<TEntity, TKey> helper, IQueryFactory queryFactory)
            : base(appController, helper, queryFactory)
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
        protected ICandidateFactory<TEntity, TKey> Helper { get; }
        protected IQueryFactory QueryFactory { get; }

        protected ReadOnlyWebController(TAppController appController, ICandidateFactory<TEntity, TKey> helper, IQueryFactory queryFactory)
        {
            AppController = appController;
            Helper = helper ?? throw new ArgumentNullException(nameof(helper));
            QueryFactory = queryFactory;
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
            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Get);

            IEnumerable<TEntity> entity = await AppController.GetAsync(query);

            return Ok(entity);
        }

        protected virtual async Task<ActionResult<TEntity>> ProtectedGetAsync(TKey id)
        {
            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity,TKey>(HttpVerbs.Get);

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
