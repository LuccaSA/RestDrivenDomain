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
        protected ReadOnlyWebController(IReadOnlyAppController<TEntity, TKey> appController, ICandidateFactory<TEntity, TKey> candidateFactory, IQueryFactory queryFactory)
            : base(appController, candidateFactory, queryFactory)
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
        protected ICandidateFactory<TEntity, TKey> CandidateFactory { get; }
        protected IQueryFactory QueryFactory { get; }

        protected ReadOnlyWebController(TAppController appController, ICandidateFactory<TEntity, TKey> candidateFactory, IQueryFactory queryFactory)
        {
            AppController = appController;
            CandidateFactory = candidateFactory ?? throw new ArgumentNullException(nameof(candidateFactory));
            QueryFactory = queryFactory;
        }

        protected virtual HttpVerbs AllowedHttpVerbs => HttpVerbs.None;

        public virtual async Task<ActionResult<IEnumerable<TEntity>>> GetAsync()
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return NotFound();
            }
            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Get);
            IEnumerable<TEntity> entity = await AppController.GetAsync(query);
            return Ok(entity);
        }

        public virtual async Task<ActionResult<TEntity>> GetByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return NotFound();
            }
            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Get);
            TEntity entity = await AppController.GetByIdAsync(id, query);
            if (entity == null)
            {
                return NotFound(id);
            }
            return Ok(entity);
        }

        protected virtual NotFoundObjectResult NotFound(TKey id) => NotFound(new { Id = id, error = $"Resource {id} not found" });
    }
}
