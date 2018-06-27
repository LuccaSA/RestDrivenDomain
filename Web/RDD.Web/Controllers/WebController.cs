using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NExtends.Primitives.Types;
using RDD.Application;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using RDD.Web.Models;

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

        public Task<IActionResult> PostAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Post))
            {
                return ProtectedPostAsync();
            }
            return Task.FromResult((IActionResult)NotFound());
        }

        public Task<IActionResult> PutByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return ProtectedPutAsync(id);
            }
            return Task.FromResult((IActionResult)NotFound());
        }

        public Task<IActionResult> PutAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return ProtectedPutAsync();
            }
            return Task.FromResult((IActionResult)NotFound());
        }

        public Task<IActionResult> DeleteByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Delete))
            {
                return ProtectedDeleteAsync(id);
            }
            return Task.FromResult((IActionResult)NotFound());
        }

        protected virtual async Task<IActionResult> ProtectedPostAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Post, false);
            ICandidate<TEntity, TKey> candidate = Helper.CreateCandidate();

            TEntity entity = await AppController.CreateAsync(candidate, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        protected virtual async Task<IActionResult> ProtectedPutAsync(TKey id)
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Put, false);
            ICandidate<TEntity, TKey> candidate = Helper.CreateCandidate();

            TEntity entity = await AppController.UpdateByIdAsync(id, candidate, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        protected virtual async Task<IActionResult> ProtectedPutAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Put, false);
            IEnumerable<ICandidate<TEntity, TKey>> candidates = Helper.CreateCandidates();

            //Datas est censé contenir un tableau d'objet ayant une prop "id" qui permet de les identifier individuellement
            if (candidates.Any(c => !c.HasId()))
            {
                throw new BadRequestException("PUT on collection implies that you provide an array of objets each of which with an id attribute");
            }

            var candidatesByIds = candidates.ToDictionary(c => c.Id);

            IEnumerable<TEntity> entities = await AppController.UpdateByIdsAsync(candidatesByIds, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntities(entities, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        protected virtual async Task<IActionResult> ProtectedDeleteAsync(TKey id)
        {
            await AppController.DeleteByIdAsync(id);

            return Ok();
        }

        protected virtual async Task<IActionResult> ProtectedDeleteAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Delete);
            IEnumerable<ICandidate<TEntity, TKey>> candidates = Helper.CreateCandidates();

            //Datas est censé contenir un tableau d'objet ayant une prop "id" qui permet de les identifier individuellement
            if (candidates.Any(c => !c.HasId()))
            {
                throw new BadRequestException("DELETE on collection implies that you provide an array of objets each of which with an id attribute");
            }

            var ids = candidates.Select(c => c.Id).ToList();

            await AppController.DeleteByIdsAsync(ids);

            return Ok();
        }
    }
}