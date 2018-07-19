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
        /// </summary>
        /// <param name="services"></param>
        public static void AddRdd(this IServiceCollection services)
        {
            // register base services
            services.AddScoped(typeof(ApiHelper<,>))
                .AddScoped<IEntitySerializer, EntitySerializer>()
                .AddScoped(typeof(IAppController<,>), typeof(AppController<,>))
                .AddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>))
                .AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddScoped<IStorageService, EFStorageService>()
                .AddScoped<IUrlProvider, UrlProvider>()
                .AddScoped<IEntitySerializer, EntitySerializer>()
                .AddScoped<IPatcherProvider, PatcherProvider>()
                .AddScoped<IHttpContextHelper, HttpContextHelper>()
                .AddScoped<IWebServicesCollection, WebServicesCollection>()

                .TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static void AddRddRights<TCombinationsHolder>(this IServiceCollection services, Func<IServiceProvider, IPrincipal> principalGetter)
            where TCombinationsHolder : class, ICombinationsHolder
        {
            services.AddScoped<IRightsService, RightsService>()
                .AddScoped(principalGetter)
                .AddScoped<ICombinationsHolder, TCombinationsHolder>();
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