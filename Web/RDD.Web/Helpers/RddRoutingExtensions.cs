using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace RDD.Web.Helpers
{
    public static class RDDRoutingExtensions
    {  
        /// <summary>
        /// Map default RDD routes (for default Get, GetById, Put, PutById, Post, DeleteById actions)
        /// </summary>
        /// <param name="routes">The route builder</param>
        /// <param name="prefix">Optional route prefix ("/api/v2" in /api/v2/MyController/42) </param>
        public static void MapRddDefaultRoutes(this IRouteBuilder routes, string prefix = null)
        {
            string cleanPrefix = !String.IsNullOrWhiteSpace(prefix) ? prefix.Trim().Trim('/') + '/' : string.Empty;

            routes.MapRoute("Get", cleanPrefix + "{controller}/", new { action = "GetAsync" });
            routes.MapRoute("GetById", cleanPrefix + "{controller}/{id}", new { action = "GetByIdAsync" });
            routes.MapRoute("Put", cleanPrefix + "{controller}/", new { action = "PutAsync" });
            routes.MapRoute("PutById", cleanPrefix + "{controller}/{id}", new { action = "PutByIdAsync" });
            routes.MapRoute("Post", cleanPrefix + "{controller}/", new { action = "PostAsync" });
            routes.MapRoute("Delete", cleanPrefix + "{controller}/{id}", new { action = "DeleteByIdAsync" });
        }
    }
}