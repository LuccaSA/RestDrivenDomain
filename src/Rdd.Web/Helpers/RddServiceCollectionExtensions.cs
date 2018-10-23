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
using Rdd.Domain.Helpers.Reflection;
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
using Rdd.Web.Serialization.Serializers;
using Rdd.Web.Serialization.UrlProviders;
using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Rdd.Web.Helpers
{
    public static class RddServiceCollectionExtensions
    {
        private static RddBuilder AddRddCore<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddOptions<RddOptions>();
            services.TryAddScoped<DbContext>(p => p.GetRequiredService<TDbContext>());

            services.TryAddSingleton(typeof(IInstanciator<>), typeof(DefaultInstanciator<>));
            services.TryAddSingleton<IPatcherProvider, PatcherProvider>();
            services.TryAddSingleton(typeof(IPatcher<>), typeof(ObjectPatcher<>));
            services.TryAddSingleton<EnumerablePatcher>();
            services.TryAddSingleton<DictionaryPatcher>();
            services.TryAddSingleton<ValuePatcher>();
            services.TryAddSingleton<DynamicPatcher>();
            services.TryAddSingleton<ObjectPatcher>();

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
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();

            services.TryAddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
            services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.TryAddScoped(typeof(IReadOnlyRestCollection<,>), typeof(ReadOnlyRestCollection<,>));
            services.TryAddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>));
            services.TryAddScoped(typeof(IReadOnlyAppController<,>), typeof(ReadOnlyAppController<,>));
            services.TryAddScoped(typeof(IAppController<,>), typeof(AppController<,>));

            services.AddHttpContextAccessor();

            // closed by default, overridable with AddRddDefaultRights
            services.TryAddSingleton(typeof(IRightExpressionsHelper<>), typeof(ClosedRightExpressionsHelper<>));

            return new RddBuilder(services)
                .ApplyRddSetupOptions();
        }

        private static RddBuilder AddRddCore<TDbContext>(this IServiceCollection services, Action<RddOptions> onConfigure)
            where TDbContext : DbContext
        {
            var builder = services.AddRddCore<TDbContext>();
            services.Configure(onConfigure);
            return builder;
        }

        public static RddBuilder AddJsonConverter(this RddBuilder rddBuilder, JsonConverter jsonConverter)
        {
            rddBuilder.JsonConverters.Add(jsonConverter);
            return rddBuilder;
        }

        private static RddBuilder ApplyRddSetupOptions(this RddBuilder rddBuilder)
        {
            rddBuilder.Services.PostConfigure<MvcJsonOptions>(o =>
            {
                foreach (JsonConverter converter in rddBuilder.JsonConverters)
                {
                    o.SerializerSettings.Converters.Add(converter);
                }
            });
            return rddBuilder;
        }

        public static RddBuilder AddRddInheritanceConfiguration<TConfig, TEntity, TKey>(this RddBuilder rddBuilder, TConfig config)
            where TConfig : class, IInheritanceConfiguration<TEntity>
            where TEntity : class, IEntityBase<TKey>
            where TKey : IEquatable<TKey>
        {
            var services = rddBuilder.Services;

            services.AddSingleton<IInheritanceConfiguration>(s => config);
            services.AddSingleton<IInheritanceConfiguration<TEntity>>(s => config);

            services.TryAddSingleton<IPatcher<TEntity>, BaseClassPatcher<TEntity>>();
            services.TryAddSingleton<IInstanciator<TEntity>, BaseClassInstanciator<TEntity>>();

            rddBuilder.AddJsonConverter(new BaseClassJsonConverter<TEntity>(config));

            return rddBuilder;
        }

        public static RddBuilder WithDefaultRights(this RddBuilder rddBuilder, RightDefaultMode mode)
        {
            switch (mode)
            {
                case RightDefaultMode.Closed:
                    rddBuilder.Services.AddSingleton(typeof(IRightExpressionsHelper<>), typeof(ClosedRightExpressionsHelper<>));
                    break;
                case RightDefaultMode.Open:
                    rddBuilder.Services.AddSingleton(typeof(IRightExpressionsHelper<>), typeof(OpenRightExpressionsHelper<>));
                    break;
                default:
                    throw new ArgumentException("Invalid right mode", nameof(mode));
            }
            return rddBuilder;
        }

        public static RddBuilder AddRddSerialization(this RddBuilder rddBuilder)
        {
            var services = rddBuilder.Services;

            services.TryAddSingleton(typeof(Inflector.Inflector), p => new Inflector.Inflector(new CultureInfo("en-US")));
            services.TryAddSingleton<IPluralizationService, PluralizationService>();

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IUrlProvider, UrlProvider>();

            services.TryAddSingleton<NamingStrategy>(new CamelCaseNamingStrategy());
            services.TryAddSingleton<IReflectionHelper, ReflectionHelper>();
            services.TryAddSingleton<ISerializerProvider, SerializerProvider>();

            services.TryAddSingleton<ArraySerializer>();
            services.TryAddSingleton<BaseClassSerializer>();
            services.TryAddSingleton<CultureInfoSerializer>();
            services.TryAddSingleton<DictionarySerializer>();
            services.TryAddSingleton<EntitySerializer>();
            services.TryAddSingleton<MetadataSerializer>();
            services.TryAddSingleton<ObjectSerializer>();
            services.TryAddSingleton<SelectionSerializer>();
            services.TryAddSingleton<ToStringSerializer>();
            services.TryAddSingleton<ValueSerializer>();

            return rddBuilder;
        }

        public static RddBuilder AddRdd<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            return services
                .AddRddCore<TDbContext>()
                .AddRddSerialization();
        }

        public static RddBuilder AddRdd<TDbContext>(this IServiceCollection services, Action<RddOptions> onConfigure)
            where TDbContext : DbContext
        {
            return services
                .AddRddCore<TDbContext>(onConfigure)
                .AddRddSerialization();
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