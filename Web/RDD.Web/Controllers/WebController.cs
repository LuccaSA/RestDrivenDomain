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
        protected WebController(IAppController<TEntity, TKey> appController, ApiHelper<TEntity, TKey> helper, IRDDSerializer rddSerializer)
            : base(appController, helper, rddSerializer)
        {
        }
    }

    public abstract class WebController<TAppController, TEntity, TKey> : ReadOnlyWebController<TAppController, TEntity, TKey>
        where TAppController : IAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {

        protected WebController(TAppController appController, ApiHelper<TEntity, TKey> helper, IRDDSerializer rddSerializer)
            : base(appController, helper, rddSerializer)
        {
        }

        public virtual async Task<IActionResult> PostAsync()
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Post))
            {
                return NotFound();
            }
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Post, false);
            ICandidate<TEntity, TKey> candidate = Helper.CreateCandidate();

            TEntity entity = await AppController.CreateAsync(candidate, query);

            return Ok(RDDSerializer.Serialize(entity, query));
        }

        public virtual async Task<IActionResult> PutByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return NotFound();
            }
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Put, false);
            ICandidate<TEntity, TKey> candidate = Helper.CreateCandidate();

            TEntity entity = await AppController.UpdateByIdAsync(id, candidate, query);

            if (entity == null)
            {
                return NotFound(id);
            }

            return Ok(RDDSerializer.Serialize(entity, query));
        }

        public virtual async Task<IActionResult> PutAsync()
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return NotFound();
            }
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Put, false);
            IEnumerable<ICandidate<TEntity, TKey>> candidates = Helper.CreateCandidates();

            if (candidates.Any(c => !c.HasId()))
            {
                return BadRequest("To edit a collection of entities, provide an array of objets with an property id");
            }

            var candidatesByIds = candidates.ToDictionary(c => c.Id);

            IEnumerable<TEntity> entities = await AppController.UpdateByIdsAsync(candidatesByIds, query);

            return Ok(RDDSerializer.Serialize(entities, query));
        }

        public virtual async Task<IActionResult> DeleteByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Delete))
            {
                return NotFound();
            }
            await AppController.DeleteByIdAsync(id);

            return Ok();
        }

        public virtual async Task<IActionResult> DeleteAsync()
        {
            if (!AllowedHttpVerbs.HasVerb(HttpVerbs.Delete))
            {
                return NotFound();
            }
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