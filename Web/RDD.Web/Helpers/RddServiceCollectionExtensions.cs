using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using RDD.Application;
using RDD.Application.Controllers;
using RDD.Domain;
using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using RDD.Infra;
using RDD.Infra.Storage;
using RDD.Web.Models;
using RDD.Web.Serialization;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.UrlProviders;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RDD.Web.Helpers
{
    public static class RDDServiceCollectionExtensions
    {
        /// <summary>
        /// Register minimum RDD dependecies. Set up RDD services via Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// IRightsService and IRDDSerialization are missing for this setup to be ready
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddRDDCore<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            // register base services
            services.TryAddScoped<DbContext, TDbContext>();
            services.TryAddScoped<IStorageService, EFStorageService>();
            services.TryAddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
            services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.TryAddScoped<IPatcherProvider, PatcherProvider>();
            services.TryAddScoped(typeof(IPatcher<>), typeof(ObjectPatcher<>));
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

        public static IServiceCollection AddRddInheritanceConfiguration<TConfig, TEntity, TKey>(this IServiceCollection services, TConfig config)
            where TConfig : class, IInheritanceConfiguration<TEntity>
            where TEntity : class, IEntityBase<TEntity, TKey>
            where TKey : IEquatable<TKey>
        {
            services.AddSingleton<IInheritanceConfiguration>(s => config);
            services.AddSingleton<IInheritanceConfiguration<TEntity>>(s => config);
            services.AddScoped<IPatcher<TEntity>, BaseClassPatcher<TEntity>>();
            services.AddScoped<IInstanciator<TEntity>, BaseClassInstanciator<TEntity>>();

            if (JsonConvert.DefaultSettings == null)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings { Converters = new List<JsonConverter> { new BaseClassJsonConverter<TEntity>(config) } };
            }
            else
            {
                var initSettings = JsonConvert.DefaultSettings;
                JsonConvert.DefaultSettings = () =>
                {
                    var result = initSettings();
                    result.Converters = result.Converters ?? new List<JsonConverter>();
                    result.Converters.Add(new BaseClassJsonConverter<TEntity>(config));
                    return result;
                };
            }

            return services;
        }

        public static IServiceCollection AddRDDRights<TCombinationsHolder, TPrincipal>(this IServiceCollection services)
            where TCombinationsHolder : class, ICombinationsHolder
            where TPrincipal : class, IPrincipal
        {
            services.TryAddScoped(typeof(IRightExpressionsHelper<>), typeof(RightExpressionsHelper<>));
            services.TryAddScoped<IPrincipal, TPrincipal>();
            services.TryAddScoped<ICombinationsHolder, TCombinationsHolder>();
            return services;
        }

        public static IServiceCollection AddRDDSerialization<TPrincipal>(this IServiceCollection services)
            where TPrincipal : class, IPrincipal
        {
            services.TryAddScoped(typeof(Inflector.Inflector), p => new Inflector.Inflector(new CultureInfo("en-US")));
            services.TryAddScoped<IPluralizationService, PluralizationService>();

            services.AddMemoryCache();
            services.TryAddScoped<IReflectionProvider, ReflectionProvider>();

            services.TryAddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<IUrlProvider, UrlProvider>();
            services.TryAddScoped<ISerializerProvider, SerializerProvider>();
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