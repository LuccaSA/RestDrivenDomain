using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Web.Models;
using Rdd.Web.Querying;
using Rdd.Web.Serialization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Web.Controllers
{
    public abstract class WebController<TEntity, TKey> : WebController<IAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected WebController(IAppController<TEntity, TKey> appController, ICandidateParser candidateParser, IQueryParser<TEntity> queryParser)
            : base(appController, candidateParser, queryParser)
        {
        }
    }

    public abstract class WebController<TAppController, TEntity, TKey> : ReadOnlyWebController<TAppController, TEntity, TKey>
        where TAppController : class, IAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected ICandidateParser CandidateParser { get; }

        protected WebController(TAppController appController, ICandidateParser candidateParser, IQueryParser<TEntity> queryParser)
            : base(appController, queryParser)
        {
            CandidateParser = candidateParser;
        }

        [HttpPost]
        public virtual async Task<IActionResult> PostAsync()
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Post))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, false);
            var candidate = await CandidateParser.ParseAsync<TEntity, TKey>(HttpContext.Request);

            TEntity entity = await AppController.CreateAsync(candidate, query);

            return new RddJsonResult<TEntity>(entity, query.Fields);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> PutByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Put))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, false);
            var candidate = await CandidateParser.ParseAsync<TEntity, TKey>(HttpContext.Request);

            TEntity entity = await AppController.UpdateByIdAsync(id, candidate, query);

            if (entity == null)
            {
                return NotFound(id);
            }

            return new RddJsonResult<TEntity>(entity, query.Fields);
        }

        [HttpPut]
        public virtual async Task<IActionResult> PutAsync()
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Put))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, true);
            var candidates = await CandidateParser.ParseManyAsync<TEntity, TKey>(HttpContext.Request);

            if (candidates.Any(c => !c.HasId()))
            {
                return BadRequest("To edit a collection of entities, provide an array of objets with an 'id' property");
            }

            var candidatesByIds = candidates.ToDictionary(c => c.Id);

            var entities = (await AppController.UpdateByIdsAsync(candidatesByIds, query)).ToList();

            return new RddJsonResult<TEntity>(new Selection<TEntity>(entities, entities.Count), query.Fields);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Delete))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, false);
            await AppController.DeleteByIdAsync(id, query);

            return Ok();
        }

        [HttpDelete]
        public virtual async Task<IActionResult> DeleteAsync()
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Delete))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            var candidates = await CandidateParser.ParseManyAsync<TEntity, TKey>(HttpContext.Request);

            if (candidates.Any(c => !c.HasId()))
            {
                return BadRequest("To delete a collection of entities, provide an array of objets with an 'id' property");
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, true);
            await AppController.DeleteByIdsAsync(candidates.Select(c => c.Id), query);

            return Ok();
        }
    }
}