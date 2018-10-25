using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Web.Models;
using Rdd.Web.Querying;
using Rdd.Web.Serialization;
using System;
using System.Threading.Tasks;

namespace Rdd.Web.Controllers
{
    public abstract class ReadOnlyWebController<TEntity, TKey> : ReadOnlyWebController<IReadOnlyRepository<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected ReadOnlyWebController(IReadOnlyRepository<TEntity, TKey> repository, IQueryParser<TEntity, TKey> queryParser, HttpQuery<TEntity, TKey> httpQuery)
            : base(repository, queryParser, httpQuery)
        {
        }
    }

    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class ReadOnlyWebController<TRepository, TEntity, TKey> : ControllerBase
        where TRepository : class, IReadOnlyRepository<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected TRepository Repository { get; }
        protected IQueryParser<TEntity, TKey> QueryParser { get; }
        protected HttpQuery<TEntity, TKey> HttpQuery { get; set; }

        protected ReadOnlyWebController(TRepository repository, IQueryParser<TEntity, TKey> queryParser, HttpQuery<TEntity, TKey> httpQuery)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            QueryParser = queryParser ?? throw new ArgumentNullException(nameof(queryParser));
            HttpQuery = httpQuery ?? throw new ArgumentNullException(nameof(httpQuery));
        }

        protected virtual HttpVerbs AllowedHttpVerbs => HttpVerbs.None;

        [HttpGet]
        public virtual async Task<IActionResult> GetAsync()
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Get))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            HttpQuery = QueryParser.Parse(HttpContext.Request, true);

            ISelection<TEntity> selection = await Repository.GetAsync();

            return new RddJsonResult<TEntity>(selection, HttpQuery.Fields);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Get))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            HttpQuery = QueryParser.Parse(HttpContext.Request, true);

            TEntity entity = await Repository.GetAsync(id);

            if (entity == null)
            {
                return NotFound(id);
            }

            return new RddJsonResult<TEntity>(entity, HttpQuery.Fields);
        }

        protected NotFoundObjectResult NotFound(TKey id) => NotFound(new { Id = id, error = $"Resource {id} not found" });
    }
}