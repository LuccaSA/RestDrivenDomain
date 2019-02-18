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
    public abstract class ReadOnlyMappedWebController<TEntityDto, TEntity, TKey> : ReadOnlyMappedWebController<IReadOnlyAppController<TEntity, TKey>, TEntityDto, TEntity, TKey>
        where TEntityDto : class
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected ReadOnlyMappedWebController(IReadOnlyAppController<TEntity, TKey> appController, IQueryParser<TEntityDto> queryParser, IRddObjectsMapper<TEntityDto, TEntity> mapper)
            : base(appController, queryParser, mapper)
        {
        }
    }

    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class ReadOnlyMappedWebController<TAppController, TEntityDto, TEntity, TKey> : ControllerBase
        where TAppController : class, IReadOnlyAppController<TEntity, TKey>
        where TEntityDto : class
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected TAppController AppController { get; }
        protected IQueryParser<TEntityDto> QueryParser { get; }
        protected IRddObjectsMapper<TEntityDto, TEntity> Mapper { get; }

        protected ReadOnlyMappedWebController(TAppController appController, IQueryParser<TEntityDto> queryParser, IRddObjectsMapper<TEntityDto, TEntity> mapper)
        {
            AppController = appController ?? throw new ArgumentNullException(nameof(appController));
            QueryParser = queryParser ?? throw new ArgumentNullException(nameof(queryParser));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected virtual HttpVerbs AllowedHttpVerbs => HttpVerbs.None;

        [HttpGet]
        public virtual async Task<IActionResult> GetAsync()
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Get))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntityDto> queryDto = QueryParser.Parse(HttpContext.Request, true);
            var query = Mapper.Map(queryDto);

            ISelection<TEntity> selection = await AppController.GetAsync(query);

            return new RddJsonResult<TEntityDto>(Mapper.Map(selection), queryDto.Fields);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetByIdAsync(TKey id)
        {
            if (!AllowedHttpVerbs.HasFlag(HttpVerbs.Get))
            {
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            Query<TEntityDto> queryDto = QueryParser.Parse(HttpContext.Request, false);
            var query = Mapper.Map(queryDto);

            TEntity entity = await AppController.GetByIdAsync(id, query);

            if (entity == null)
            {
                return NotFound(id);
            }

            return new RddJsonResult<TEntityDto>(Mapper.Map(entity), queryDto.Fields);
        }

        protected NotFoundObjectResult NotFound(TKey id) => NotFound(new { Id = id, error = $"Resource {id} not found" });
    }
}