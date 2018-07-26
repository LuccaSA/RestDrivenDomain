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

namespace RDD.Web.Controllers
{
    public abstract class WebController<TEntity, TKey> : WebController<IAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected WebController(IAppController<TEntity, TKey> appController, ApiHelper<TEntity, TKey> helper) 
            : base(appController, helper)
        {
        }
    }

    public abstract class WebController<TAppController, TEntity, TKey> : ReadOnlyWebController<TAppController, TEntity, TKey>
        where TAppController : IAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected WebController(TAppController appController, ApiHelper<TEntity, TKey> helper)
            : base(appController, helper)
        {
        }

        public Task<ActionResult<TEntity>> PostAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Post))
            {
                return ProtectedPostAsync();
            }
            return Task.FromResult((ActionResult<TEntity>)NotFound());
        }

        public Task<ActionResult<TEntity>> PutByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return ProtectedPutAsync(id);
            }
            return Task.FromResult((ActionResult<TEntity>)NotFound());
        }

        public Task<ActionResult<IReadOnlyCollection<TEntity>>> PutAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return ProtectedPutAsync();
            }
            return Task.FromResult((ActionResult<IReadOnlyCollection<TEntity>>)NotFound());
        }

        public Task<ActionResult> DeleteByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Delete))
            {
                return ProtectedDeleteAsync(id);
            }
            return Task.FromResult((ActionResult)NotFound());
        }

        protected virtual async Task<ActionResult<TEntity>> ProtectedPostAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Post, false);
            ICandidate<TEntity, TKey> candidate = Helper.CreateCandidate();

            TEntity entity = await AppController.CreateAsync(candidate, query);

            return Ok(entity);
        }

        protected virtual async Task<ActionResult<TEntity>> ProtectedPutAsync(TKey id)
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Put, false);
            ICandidate<TEntity, TKey> candidate = Helper.CreateCandidate();

            TEntity entity = await AppController.UpdateByIdAsync(id, candidate, query);
            if (entity == null)
            {
                return NotFound(id);
            }
            return Ok(entity);
        }

        protected virtual async Task<ActionResult<IReadOnlyCollection<TEntity>>> ProtectedPutAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Put, false);
            IEnumerable<ICandidate<TEntity, TKey>> candidates = Helper.CreateCandidates();

            if (candidates.Any(c => !c.HasId()))
            {
                return BadRequest("To edit a collection of entities, provide an array of objets with an property id");
            }

            var candidatesByIds = candidates.ToDictionary(c => c.Id);

            IReadOnlyCollection<TEntity> entities = await AppController.UpdateByIdsAsync(candidatesByIds, query);

            return Ok(entities);
        }

        protected virtual async Task<ActionResult> ProtectedDeleteAsync(TKey id)
        {
            await AppController.DeleteByIdAsync(id);

            return Ok();
        }

        protected virtual async Task<IActionResult> ProtectedDeleteAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Delete);
            IEnumerable<ICandidate<TEntity, TKey>> candidates = Helper.CreateCandidates();

            if (candidates.Any(c => !c.HasId()))
            {
                return BadRequest("To delete a collection of entities, provide an array of objets with an property id");
            }

            var ids = candidates.Select(c => c.Id).ToList();

            await AppController.DeleteByIdsAsync(ids);

            return Ok();
        }
    }
}