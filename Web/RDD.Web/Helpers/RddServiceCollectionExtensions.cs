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
using System.Collections.Generic;

namespace RDD.Web.Helpers
{
    public static class RDDServiceCollectionExtensions
    {
        private static void RegisterBoundedContextSetup(IServiceCollection services, List<Type> dbContextTypes)
        {
            BoundedContextsResolver.DbContextTypes = dbContextTypes;
            services.AddSingleton<BoundedContextsResolver>();

            foreach (var dbContextType in dbContextTypes)
            {
                services.TryAddScoped(dbContextType);
            }
        }

        private static void RegisterMonoContextSetup<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddSingleton<MonoContextResolver<TDbContext>>();
            services.TryAddScoped<TDbContext>();
        }

        private static void RegisterRDDCore(IServiceCollection services)
        {
            // register base services
            services.TryAddScoped(typeof(IStorageService<>), typeof(EFStorageService<>));
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
        }
    
        /// <summary>
        /// Register minimum RDD dependecies. Set up RDD services via Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// IRightsService and IRDDSerialization are missing for this setup to be ready
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddRDDCoreWithBoundedContext(this IServiceCollection services, List<Type> dbContextTypes)
        {
            RegisterBoundedContextSetup(services, dbContextTypes);
            RegisterRDDCore(services);
            return services;
        }

        /// <summary>
        /// Register minimum RDD dependecies. Set up RDD services via Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// IRightsService and IRDDSerialization are missing for this setup to be ready
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddRDDCore<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            RegisterMonoContextSetup<TDbContext>(services);
            RegisterRDDCore(services);
            return services;
        }

        public static IServiceCollection AddRDDRights<TCombinationsHolder, TPrincipal>(this IServiceCollection services)
            where TCombinationsHolder : class, ICombinationsHolder
            where TPrincipal : class, IPrincipal
        {
            services.TryAddScoped<IRightExpressionsHelper, RightExpressionsHelper>();
            services.TryAddScoped<IPrincipal, TPrincipal>();
            services.TryAddScoped<ICombinationsHolder, TCombinationsHolder>();
            return services;
        }

        public static IServiceCollection AddRDDSerialization<TPrincipal>(this IServiceCollection services)
            where TPrincipal : class, IPrincipal
        {
            services.TryAddScoped<IUrlProvider, UrlProvider>();
            services.TryAddScoped<IEntitySerializer, EntitySerializer>();
            services.TryAddScoped<IRDDSerializer, RDDSerializer>();
            services.TryAddScoped<IPrincipal, TPrincipal>();
            return services;
        }

        public static IServiceCollection AddRDD<TDbContext, TCombinationsHolder, TPrincipal>(this IServiceCollection services)
            where TDbContext : DbContext
            where TCombinationsHolder : class, ICombinationsHolder
            where TPrincipal : class, IPrincipal
        {
            return services.AddRDDCore<TDbContext>()
                .AddRDDRights<TCombinationsHolder, TPrincipal>()
                .AddRDDSerialization<TPrincipal>();
        }

        /// <summary>
        /// Register RDD middleware in the pipeline request
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRDD(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HttpStatusCodeExceptionMiddleware>();
        }
    }

}