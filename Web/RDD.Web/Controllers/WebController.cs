using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using RDD.Web.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDD.Web.Querying;

namespace RDD.Web.Controllers
{
    public abstract class WebController<TEntity, TKey> : WebController<IAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected WebController(IAppController<TEntity, TKey> appController, ICandidateFactory<TEntity, TKey> helper, IQueryFactory queryFactory)
            : base(appController, helper, queryFactory)
        {
        }
    }

    public abstract class WebController<TAppController, TEntity, TKey> : ReadOnlyWebController<TAppController, TEntity, TKey>
        where TAppController : IAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {

        protected WebController(TAppController appController, ICandidateFactory<TEntity, TKey> helper, IQueryFactory queryFactory)
            : base(appController, helper, queryFactory)
        {
        }

        public virtual async Task<ActionResult<TEntity>> PostAsync()
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Post))
            {
                return NotFound();
            }

            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Post);
            ICandidate<TEntity, TKey> candidate = CandidateFactory.CreateCandidate();
            TEntity entity = await AppController.CreateAsync(candidate, query);
            return Ok(entity);
        }

        public virtual async Task<ActionResult<TEntity>> PutByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return NotFound();
            }

            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Put);
            ICandidate<TEntity, TKey> candidate = CandidateFactory.CreateCandidate();
            TEntity entity = await AppController.UpdateByIdAsync(id, candidate, query);
            if (entity == null)
            {
                return NotFound(id);
            }
            return Ok(entity);
        }

        public virtual async Task<ActionResult<IEnumerable<TEntity>>> PutAsync()
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return NotFound();
            }

            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Put);
            IEnumerable<ICandidate<TEntity, TKey>> candidates = CandidateFactory.CreateCandidates();
            if (candidates.Any(c => !c.HasId()))
            {
                return BadRequest("To edit a collection of entities, provide an array of objects with an property id");
            }
            var candidatesByIds = candidates.ToDictionary(c => c.Id);
            IEnumerable<TEntity> entities = await AppController.UpdateByIdsAsync(candidatesByIds, query);
            return Ok(entities);
        }

        public virtual async Task<ActionResult> DeleteByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Delete))
            {
                return NotFound();
            }

            await AppController.DeleteByIdAsync(id);
            return Ok();
        }

        public virtual async Task<ActionResult> DeleteAsync()
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Delete))
            {
                return NotFound();
            }

            IEnumerable<ICandidate<TEntity, TKey>> candidates = CandidateFactory.CreateCandidates();
            if (candidates.Any(c => !c.HasId()))
            {
                return BadRequest("To delete a collection of entities, provide an array of objects with an property id");
            }
            var ids = candidates.Select(c => c.Id).ToList();
            await AppController.DeleteByIdsAsync(ids);
            return Ok();
        }

    }
}