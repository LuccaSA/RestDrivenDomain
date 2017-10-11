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
    public abstract class WebController<TAppController, TEntity, TKey> : ReadOnlyWebController<TAppController, TEntity, TKey>
        where TAppController : IAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected WebController(TAppController appController, ApiHelper<TEntity, TKey> helper)
            : base(appController, helper)
        {
        }

        protected virtual async Task<IActionResult> ProtectedPostAsync()
        {
            Helper.WebContextWrapper.SetContext(HttpContext);

            Query<TEntity> query = Helper.CreateQuery(HttpVerb.POST, false);
            PostedData datas = Helper.InputObjectsFromIncomingHttpRequest().SingleOrDefault();

            TEntity entity = await AppController.CreateAsync(datas, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        protected virtual async Task<IActionResult> ProtectedPutAsync(TKey id)
        {
            Helper.WebContextWrapper.SetContext(HttpContext);

            Query<TEntity> query = Helper.CreateQuery(HttpVerb.PUT, false);
            PostedData datas = Helper.InputObjectsFromIncomingHttpRequest().SingleOrDefault();

            TEntity entity = await AppController.UpdateByIdAsync(id, datas, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        protected virtual async Task<IActionResult> ProtectedPutAsync()
        {
            Helper.WebContextWrapper.SetContext(HttpContext);
            Query<TEntity> query = Helper.CreateQuery(HttpVerb.PUT, false);
            List<PostedData> datas = Helper.InputObjectsFromIncomingHttpRequest();

            //Datas est censé contenir un tableau d'objet ayant une prop "id" qui permet de les identifier individuellement
            if (datas.Any(d => !d.ContainsKey("id")))
            {
                throw new HttpLikeException(HttpStatusCode.BadRequest, "PUT on collection implies that you provide an array of objets each of which with an id attribute");
            }

            Dictionary<TKey, PostedData> datasByIds;

            //Il faut que les id soient convertibles en TKey
            try
            {
                datasByIds = datas.ToDictionary(el => (TKey) TypeExtensions.Convert<TKey>(el["id"].Value), el => el);
            }
            catch
            {
                throw new HttpLikeException(HttpStatusCode.BadRequest, string.Format("PUT on collection implies that each id be of type : {0}", typeof(TKey).Name));
            }

            IEnumerable<TEntity> entities = await AppController.UpdateByIdsAsync(datasByIds, query);

            var dataContainer = new Metadata(Helper.Serializer.SerializeEntities(entities, query.Fields), query.Options, query.Page, Helper.Execution);

            return Ok(dataContainer.ToDictionary());
        }

        protected virtual async Task<IActionResult> ProtectedDeleteAsync(TKey id)
        {
            Helper.WebContextWrapper.SetContext(HttpContext);

            await AppController.DeleteByIdAsync(id);

            return Ok();
        }

        protected virtual async Task<IActionResult> ProtectedDeleteAsync()
        {
            Helper.WebContextWrapper.SetContext(HttpContext);
            Query<TEntity> query = Helper.CreateQuery(HttpVerb.DELETE);

            List<PostedData> datas = Helper.InputObjectsFromIncomingHttpRequest();

            if (datas.Any(d => !d.ContainsKey("id")))
            {
                throw new HttpLikeException(HttpStatusCode.BadRequest, "DELETE on collection implies that you provide an array of objets each of which with an id attribute");
            }

            IList<TKey> ids;

            //Il faut que les id soient convertibles en TKey
            try
            {
                ids = datas.Select(el => (TKey) TypeExtensions.Convert<TKey>(el["id"].Value)).ToList();
            }
            catch
            {
                throw new HttpLikeException(HttpStatusCode.BadRequest, string.Format("DELETE on collection implies that each id be of type : {0}", typeof(TKey).Name));
            }

            await AppController.DeleteByIdsAsync(ids);

            return Ok();
        }
    }
}