using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using RDD.Web.Serialization;
using System;
using System.Threading.Tasks;

namespace RDD.Web.Controllers
{
    public abstract class ReadOnlyWebController<TEntity, TKey> : ReadOnlyWebController<IReadOnlyAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected ReadOnlyWebController(IReadOnlyAppController<TEntity, TKey> appController, ApiHelper<TEntity, TKey> helper, IRDDSerializer rddSerializer)
            : base(appController, helper, rddSerializer)
        {
        }
    }

    public abstract class ReadOnlyWebController<TAppController, TEntity, TKey> : ControllerBase
        where TAppController : IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected TAppController AppController { get; }
        protected ApiHelper<TEntity, TKey> Helper { get; }
        protected IRDDSerializer _rddSerializer;

        protected ReadOnlyWebController(TAppController appController, ApiHelper<TEntity, TKey> helper, IRDDSerializer rddSerializer)
        {
            AppController = appController;
            Helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _rddSerializer = rddSerializer ?? throw new ArgumentNullException(nameof(rddSerializer));
        }

        protected virtual HttpVerbs AllowedHttpVerbs => HttpVerbs.None;

        public Task<IActionResult> GetAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return ProtectedGetAsync();
            }
            return Task.FromResult((IActionResult)NotFound());
        }

        public Task<IActionResult> GetByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Get))
            {
                return ProtectedGetAsync(id);
            }
            return Task.FromResult((IActionResult)NotFound());
        }

        protected virtual async Task<IActionResult> ProtectedGetAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get);

            ISelection<TEntity> selection = await AppController.GetAsync(query);

            return Ok(_rddSerializer.Serialize(selection, query));
        }

        protected virtual async Task<IActionResult> ProtectedGetAsync(TKey id)
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Get, false);

            TEntity entity = await AppController.GetByIdAsync(id, query);

            return Ok(_rddSerializer.Serialize(entity, query));
        }
    }
}