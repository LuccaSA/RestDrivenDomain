using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rdd.Web.Helpers
{
    public static class RddServiceCollectionExtensions
    {
        public static IServiceCollection AddRdd<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
            => services.AddRdd<TDbContext>(null);

        public static IServiceCollection AddRdd<TDbContext>(this IServiceCollection services, Action<RddBuilder> configureRdd)
            where TDbContext : DbContext
        {
            var builder = new RddBuilder(services);

            if (configureRdd != null)
            {
                configureRdd(builder);
            }

            return builder.Build<TDbContext>();
        }

        /// <summary>
        /// Register Rdd middleware in the pipeline request
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRdd(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HttpStatusCodeExceptionMiddleware>();
        }
    }
}