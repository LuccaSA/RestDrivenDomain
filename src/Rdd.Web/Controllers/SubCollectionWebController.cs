using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Web.Models;
using Rdd.Web.Querying;
using Rdd.Web.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rdd.Web.Controllers
{
    public class SubCollectionControllerAttribute : Attribute, IControllerModelConvention
    {
        private readonly string _parentController;
        private readonly string _propertyName;

        public SubCollectionControllerAttribute(string parentController, string propertyName)
        {
            _parentController = parentController ?? throw new ArgumentNullException(nameof(parentController));
            _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        }

        public void Apply(ControllerModel controllerModel)
        {
            controllerModel.Properties["parentController"] = _parentController;
            controllerModel.Properties["propertyName"] = _propertyName;
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [SubCollectionController("default", "subProperty")]
    [Route("api/{parentController}")]
    public abstract class SubCollectionWebController<TEntity, TKey, TParentKey> : ControllerBase
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected abstract string Key { get; }
        protected abstract Expression<Func<TEntity, TParentKey>> ParentId { get; }
        protected virtual HttpVerbs AllowedHttpVerbs => HttpVerbs.None;

        protected IAppController<TEntity, TKey> AppController { get; }
        protected ICandidateParser CandidateParser { get; }
        protected ISubCollectionQueryParser<TEntity, TParentKey> QueryParser { get; }

        protected SubCollectionWebController(IAppController<TEntity, TKey> appController, ICandidateParser candidateParser, ISubCollectionQueryParser<TEntity, TParentKey> queryParser)
        {
            AppController = appController ?? throw new ArgumentNullException(nameof(appController));
            CandidateParser = candidateParser ?? throw new ArgumentNullException(nameof(candidateParser));
            QueryParser = queryParser ?? throw new ArgumentNullException(nameof(queryParser));
        }

        [HttpGet("{parentKey}/{propertyName}")]
        public virtual async Task<IActionResult> GetAsync(TParentKey parentKey)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Get))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, parentKey, ParentId, true);

            ISelection<TEntity> selection = await AppController.GetAsync(query);

            return new RddJsonResult<TEntity>(selection, query.Fields);
        }

        [HttpGet("{parentKey}/{propertyName}/{id}")]
        public virtual async Task<IActionResult> GetByIdAsync(TParentKey parentKey, TKey id)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Get))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, parentKey, ParentId, false);

            TEntity entity = await AppController.GetByIdAsync(id, query);

            if (entity == null)
            {
                return NotFound(id);
            }

            return new RddJsonResult<TEntity>(entity, query.Fields);
        }

        protected NotFoundObjectResult NotFound(TKey id) => NotFound(new { Id = id, error = $"Resource {id} not found" });

        [HttpPost("{parentKey}/{propertyName}")]
        public virtual async Task<IActionResult> PostAsync(TParentKey parentKey)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Post))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, parentKey, ParentId, false);
            var candidate = await CandidateParser.ParseAsync<TEntity, TKey>(HttpContext.Request);

            TEntity entity = await AppController.CreateAsync(candidate, query);

            return new RddJsonResult<TEntity>(entity, query.Fields);
        }

        [HttpPut("{parentKey}/{propertyName}/{id}")]
        public virtual async Task<IActionResult> PutByIdAsync(TParentKey parentKey, TKey id)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Put))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, parentKey, ParentId, false);
            var candidate = await CandidateParser.ParseAsync<TEntity, TKey>(HttpContext.Request);

            TEntity entity = await AppController.UpdateByIdAsync(id, candidate, query);

            if (entity == null)
            {
                return NotFound(id);
            }

            return new RddJsonResult<TEntity>(entity, query.Fields);
        }

        [HttpPut("{parentKey}/{propertyName}")]
        public virtual async Task<IActionResult> PutAsync(TParentKey parentKey)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Put))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, parentKey, ParentId, true);
            var candidates = await CandidateParser.ParseManyAsync<TEntity, TKey>(HttpContext.Request);

            if (candidates.Any(c => !c.HasId()))
            {
                return BadRequest("To edit a collection of entities, provide an array of objets with an 'id' property");
            }

            var candidatesByIds = candidates.ToDictionary(c => c.Id);

            var entities = (await AppController.UpdateByIdsAsync(candidatesByIds, query)).ToList();

            return new RddJsonResult<TEntity>(new Selection<TEntity>(entities, entities.Count), query.Fields);
        }

        [HttpDelete("{parentKey}/{propertyName}/{id}")]
        public virtual async Task<IActionResult> DeleteByIdAsync(TParentKey parentKey, TKey id)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Delete))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            await AppController.DeleteByIdAsync(id);

            return Ok();
        }

        [HttpDelete("{parentKey}/{propertyName}")]
        public virtual async Task<IActionResult> DeleteAsync(TParentKey parentKey)
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

            await AppController.DeleteByIdsAsync(candidates.Select(c => c.Id));

            return Ok();
        }
    }
}