using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Web.Querying;
using Rdd.Web.Serialization;
using System;
using System.Threading.Tasks;

namespace Rdd.Web.Controllers
{
    public abstract class ReadOnlyWebController<TEntity, TKey> : ReadOnlyWebController<IReadOnlyAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected ReadOnlyWebController(IReadOnlyAppController<TEntity, TKey> appController, IQueryParser<TEntity> queryParser)
            : base(appController, queryParser)
        {
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class ReadOnlyWebController<TAppController, TEntity, TKey> : ControllerBase
        where TAppController : class, IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected TAppController AppController { get; }
        protected IQueryParser<TEntity> QueryParser { get; }

        protected ReadOnlyWebController(TAppController appController, IQueryParser<TEntity> queryParser)
        {
            AppController = appController ?? throw new ArgumentNullException(nameof(appController));
            QueryParser = queryParser ?? throw new ArgumentNullException(nameof(queryParser));
        }

        protected virtual HttpVerbs AllowedHttpVerbs => HttpVerbs.None;

        [HttpGet]
        public virtual async Task<IActionResult> Get()
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Get))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, true);

            ISelection<TEntity> selection = await AppController.GetAsync(query);

            return new RddJsonResult<TEntity>(selection, query.Fields);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(TKey id)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Get))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntity> query = QueryParser.Parse(HttpContext.Request, true);

            TEntity entity = await AppController.GetByIdAsync(id, query);

            if (entity == null)
            {
                return NotFound(id);
            }

            return new RddJsonResult<TEntity>(entity, query.Fields);
        }

        protected NotFoundObjectResult NotFound(TKey id) => NotFound(new { Id = id, error = $"Resource {id} not found" });
    }
}