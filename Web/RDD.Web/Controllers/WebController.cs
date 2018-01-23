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
            PostedData datas = Helper.InputObjectsFromIncomingHttpRequest().SingleOrDefault();

            TEntity entity = await AppController.CreateAsync(datas, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        protected virtual async Task<IActionResult> ProtectedPutAsync(TKey id)
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Put, false);
            PostedData datas = Helper.InputObjectsFromIncomingHttpRequest().SingleOrDefault();

            TEntity entity = await AppController.UpdateByIdAsync(id, datas, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        protected virtual async Task<IActionResult> ProtectedPutAsync()
        {
            Query<TEntity> query = Helper.CreateQuery(HttpVerbs.Put, false);
            List<PostedData> datas = Helper.InputObjectsFromIncomingHttpRequest();

            //Datas est censé contenir un tableau d'objet ayant une prop "id" qui permet de les identifier individuellement
            if (datas.Any(d => !d.ContainsKey("id")))
            {
                throw new BadRequestException("PUT on collection implies that you provide an array of objets each of which with an id attribute");
            }

            Dictionary<TKey, PostedData> datasByIds;

            //Il faut que les id soient convertibles en TKey
            try
            {
                datasByIds = datas.ToDictionary(el => (TKey) TypeExtensions.Convert<TKey>(el["id"].Value), el => el);
            }
            catch
            {
                throw new BadRequestException(string.Format("PUT on collection implies that each id be of type : {0}", typeof(TKey).Name));
            }

            IEnumerable<TEntity> entities = await AppController.UpdateByIdsAsync(datasByIds, query);

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

            List<PostedData> datas = Helper.InputObjectsFromIncomingHttpRequest();

            if (datas.Any(d => !d.ContainsKey("id")))
            {
                throw new BadRequestException("DELETE on collection implies that you provide an array of objets each of which with an id attribute");
            }

            IList<TKey> ids;

            //Il faut que les id soient convertibles en TKey
            try
            {
                ids = datas.Select(el => (TKey) TypeExtensions.Convert<TKey>(el["id"].Value)).ToList();
            }
            catch
            {
                throw new BadRequestException(string.Format("DELETE on collection implies that each id be of type : {0}", typeof(TKey).Name));
            }

            await AppController.DeleteByIdsAsync(ids);

            return Ok();
        }
    }
}