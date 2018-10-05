using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RDD.Application;
using RDD.Application.Controllers;
using RDD.Domain;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using RDD.Infra.Helpers;
using RDD.Infra.Storage;
using RDD.Web.Models;
using RDD.Web.Querying;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.Serializers;
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
        /// DbContext, IRightsService and IRDDSerialization are missing for this setup to be functional
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddRDDCore<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            services.TryAddScoped<DbContext>(p => p.GetService<TDbContext>());

            services.TryAddSingleton(typeof(IInstanciator<>), typeof(DefaultInstanciator<>));
            services.TryAddSingleton<IPatcherProvider, PatcherProvider>();
            services.TryAddSingleton(typeof(IPatcher<>), typeof(ObjectPatcher<>));

            services.TryAddSingleton<IJsonParser, JsonParser>();
            services.TryAddSingleton<ICandidateParser, CandidateParser>();
            services.TryAddSingleton<IStringConverter, StringConverter>();
            services.TryAddSingleton<IExpressionParser, ExpressionParser>();
            services.TryAddSingleton(typeof(IWebFilterConverter<>), typeof(WebFilterConverter<>));
            services.TryAddSingleton(typeof(IQueryParser<>), typeof(QueryParser<>));

            services.TryAddScoped<IStorageService, EFStorageService>();
            services.TryAddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
            services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.TryAddScoped(typeof(IReadOnlyRestCollection<,>), typeof(ReadOnlyRestCollection<,>));
            services.TryAddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>));
            services.TryAddScoped(typeof(IReadOnlyAppController<,>), typeof(ReadOnlyAppController<,>));
            services.TryAddScoped(typeof(IAppController<,>), typeof(AppController<,>));

            return services;
        }

        public static IServiceCollection AddRddInheritanceConfiguration<TConfig, TEntity, TKey>(this IServiceCollection services, TConfig config)
            where TConfig : class, IInheritanceConfiguration<TEntity>
            where TEntity : class, IEntityBase<TEntity, TKey>
            where TKey : IEquatable<TKey>
        {
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IInheritanceConfiguration), config));
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IInheritanceConfiguration<TEntity>), config));
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

            //scoped
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
        /// Register RDD middleware in the pipeline request BEFORE UseMVC
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRDD(this IApplicationBuilder app)
        {
            return app
                .UseMiddleware<EnableRequestRewindMiddleware>()
                .UseMiddleware<HttpStatusCodeExceptionMiddleware>();
        }
    }
}