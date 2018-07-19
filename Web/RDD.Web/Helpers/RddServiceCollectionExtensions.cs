using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
        public static IServiceCollection AddRddMinimum(this IServiceCollection services)
        {
            // register base services
            services.TryAddScoped<IStorageService, EFStorageService>();
            services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.TryAddScoped<IPatcherProvider, PatcherProvider>();
            services.TryAddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>));
            services.TryAddScoped(typeof(IAppController<,>), typeof(AppController<,>));
            services.TryAddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<IHttpContextHelper, HttpContextHelper>();
            services.TryAddScoped(typeof(ApiHelper<,>));
            return services;
        }

        public static IServiceCollection AddRddRights<TCombinationsHolder>(this IServiceCollection services, Func<IServiceProvider, IPrincipal> principalGetter)
            where TCombinationsHolder : class, ICombinationsHolder
        {
            services.TryAddScoped<IRightsService, RightsService>();
            services.TryAddScoped(principalGetter);
            services.TryAddScoped<ICombinationsHolder, TCombinationsHolder>();
            return services;
        }

        public static IServiceCollection AddRddSerialization(this IServiceCollection services, Func<IServiceProvider, IPrincipal> principalGetter)
        {
            services.TryAddScoped<IUrlProvider, UrlProvider>();
            services.TryAddScoped<IEntitySerializer, EntitySerializer>();
            services.TryAddScoped<IRddSerializer, RddSerializer>();
            services.TryAddScoped(principalGetter);
            return services;
        }

        public static IServiceCollection AddRdd<TCombinationsHolder>(this IServiceCollection services, Func<IServiceProvider, IPrincipal> principalGetter)
            where TCombinationsHolder : class, ICombinationsHolder
        {
            return services.AddRddMinimum().AddRddRights<TCombinationsHolder>(principalGetter).AddRddSerialization(principalGetter);
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