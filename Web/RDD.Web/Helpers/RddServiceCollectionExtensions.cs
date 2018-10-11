using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Application;
using Rdd.Application.Controllers;
using Rdd.Domain;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Infra.Storage;
using Rdd.Web.Models;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.Reflection;
using Rdd.Web.Serialization.Serializers;
using Rdd.Web.Serialization.UrlProviders;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Rdd.Web.Helpers
{
    public static class RddServiceCollectionExtensions
    {
        /// <summary>
        /// Register minimum Rdd dependecies. Set up Rdd services via Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// DbContext, IRightsService and IRddSerialization are missing for this setup to be functional
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddRddCore<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            services.TryAddScoped<DbContext>(p => p.GetService<TDbContext>());

            // register base services
            services.TryAddScoped<EFStorageService>();
            services.TryAddScoped<IStorageService>(s => s.GetService<EFStorageService>());
            services.TryAddScoped<IUnitOfWork>(s => s.GetService<EFStorageService>());

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
            services.TryAddSingleton<IPatcher<TEntity>, BaseClassPatcher<TEntity>>();
            services.TryAddSingleton<IInstanciator<TEntity>, BaseClassInstanciator<TEntity>>();

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

        public static IServiceCollection AddRddDefaultRights(this IServiceCollection services, RightDefaultMode mode)
        {
            switch (mode)
            {
                case RightDefaultMode.Closed:
                    services.TryAddSingleton(typeof(IRightExpressionsHelper<>), typeof(ClosedRightExpressionsHelper<>));
                    break;
                case RightDefaultMode.Open:
                    services.TryAddSingleton(typeof(IRightExpressionsHelper<>), typeof(OpenRightExpressionsHelper<>));
                    break;
            }
            return services;
        }

        public static IServiceCollection AddRddSerialization(this IServiceCollection services)
        {
            //singletons
            services.TryAddSingleton(typeof(Inflector.Inflector), p => new Inflector.Inflector(new CultureInfo("en-US")));
            services.TryAddSingleton<IPluralizationService, PluralizationService>();

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IUrlProvider, UrlProvider>();

            services.AddMemoryCache();
            services.TryAddSingleton<IReflectionProvider, ReflectionProvider>();

            services.TryAddSingleton<NamingStrategy>(new CamelCaseNamingStrategy());
            services.TryAddSingleton<ISerializerProvider, SerializerProvider>();

            services.TryAddSingleton<ArraySerializer>();
            services.TryAddSingleton<CultureInfoSerializer>();
            services.TryAddSingleton<DateTimeSerializer>();
            services.TryAddSingleton<DictionarySerializer>();
            services.TryAddSingleton<MetadataSerializer>();
            services.TryAddSingleton<SelectionSerializer>();
            services.TryAddSingleton<ToStringSerializer>();
            services.TryAddSingleton<ValueSerializer>();

            return services;
        }

        public static IServiceCollection AddRdd<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
            => services.AddRdd<TDbContext>(RightDefaultMode.Closed);

        public static IServiceCollection AddRdd<TDbContext>(this IServiceCollection services, RightDefaultMode mode)
            where TDbContext : DbContext
        {
            return services
                .AddRddCore<TDbContext>()
                .AddRddDefaultRights(mode)
                .AddRddSerialization();
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