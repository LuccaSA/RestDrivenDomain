using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Application;
using Rdd.Application.Controllers;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Json;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Infra.Helpers;
using Rdd.Infra.Storage;
using Rdd.Web.Models;
using Rdd.Web.Querying;
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

            services.TryAddSingleton(typeof(IInstanciator<>), typeof(DefaultInstanciator<>));
            services.TryAddSingleton<IPatcherProvider, PatcherProvider>();
            services.TryAddSingleton(typeof(IPatcher<>), typeof(ObjectPatcher<>));

            services.TryAddSingleton<IJsonParser, JsonParser>();
            services.TryAddSingleton<ICandidateParser, CandidateParser>();
            services.TryAddSingleton<IStringConverter, StringConverter>();
            services.TryAddSingleton<IExpressionParser, ExpressionParser>();
            services.TryAddSingleton(typeof(IWebFilterConverter<>), typeof(WebFilterConverter<>));
            services.TryAddSingleton<IPagingParser, PagingParser>();
            services.TryAddSingleton<IFilterParser, FilterParser>();
            services.TryAddSingleton<IFieldsParser, FieldsParser>();
            services.TryAddSingleton<IOrderByParser, OrderByParser>();
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

        public static IServiceCollection AddRddRights<TCombinationsHolder, TPrincipal>(this IServiceCollection services)
            where TCombinationsHolder : class, ICombinationsHolder
            where TPrincipal : class, IPrincipal
        {
            services.TryAddScoped(typeof(IRightExpressionsHelper<>), typeof(RightExpressionsHelper<>));
            services.TryAddScoped<IPrincipal, TPrincipal>();
            services.TryAddScoped<ICombinationsHolder, TCombinationsHolder>();
            return services;
        }

        public static IServiceCollection AddRddSerialization<TPrincipal>(this IServiceCollection services)
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
        /// Register Rdd middleware in the pipeline request BEFORE UseMVC
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRdd(this IApplicationBuilder app)
        {
            return app
                .UseMiddleware<EnableRequestRewindMiddleware>()
                .UseMiddleware<HttpStatusCodeExceptionMiddleware>();
        }
    }
}