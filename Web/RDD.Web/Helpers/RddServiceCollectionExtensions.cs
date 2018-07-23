using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RDD.Application;
using RDD.Application.Controllers;
using RDD.Domain;
using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using RDD.Domain.WebServices;
using RDD.Infra;
using RDD.Infra.Storage;
using RDD.Web.Serialization;
using System;

namespace RDD.Web.Helpers
{
    public static class RddServiceCollectionExtensions
    {
        /// <summary>
        /// Register minimum RDD dependecies. Set up RDD services via Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// IRightsService and IRddSerialization are missing for this setup to be ready
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddRddCore<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            // register base services
            services.TryAddScoped<DbContext, TDbContext>();
            services.TryAddScoped<IStorageService, EFStorageService>();
            services.TryAddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
            services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.TryAddScoped<IPatcherProvider, PatcherProvider>();
            services.TryAddScoped(typeof(IReadOnlyRestCollection<,>), typeof(ReadOnlyRestCollection<,>));
            services.TryAddScoped(typeof(IInstanciator<>), typeof(DefaultInstanciator<>));
            services.TryAddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>));
            services.TryAddScoped(typeof(IReadOnlyAppController<,>), typeof(ReadOnlyAppController<,>));
            services.TryAddScoped(typeof(IAppController<,>), typeof(AppController<,>));
            services.TryAddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<IHttpContextHelper, HttpContextHelper>();
            services.TryAddScoped(typeof(ApiHelper<,>));
            return services;
        }

        public static IServiceCollection AddRddRights<TCombinationsHolder, TPrincipal>(this IServiceCollection services)
            where TCombinationsHolder : class, ICombinationsHolder
            where TPrincipal : class, IPrincipal
        {
            services.TryAddScoped<IRightExpressionsHelper, RightExpressionsHelper>();
            services.TryAddScoped<IPrincipal, TPrincipal>();
            services.TryAddScoped<ICombinationsHolder, TCombinationsHolder>();
            return services;
        }

        public static IServiceCollection AddRddSerialization<TPrincipal>(this IServiceCollection services)
            where TPrincipal : class, IPrincipal
        {
            services.TryAddScoped<IUrlProvider, UrlProvider>();
            services.TryAddScoped<IEntitySerializer, EntitySerializer>();
            services.TryAddScoped<IRddSerializer, RddSerializer>();
            services.TryAddScoped<IPrincipal, TPrincipal>();
            return services;
        }

        public static IServiceCollection AddRdd<TDbContext, TCombinationsHolder, TPrincipal>(this IServiceCollection services)
            where TDbContext : DbContext
            where TCombinationsHolder : class, ICombinationsHolder
            where TPrincipal : class, IPrincipal
        {
            return services.AddRddCore<TDbContext>()
                .AddRddRights<TCombinationsHolder, TPrincipal>()
                .AddRddSerialization<TPrincipal>();
        }

        /// <summary>
        /// Register RDD middleware in the pipeline request
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRdd(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HttpStatusCodeExceptionMiddleware>();
        }
    }

}