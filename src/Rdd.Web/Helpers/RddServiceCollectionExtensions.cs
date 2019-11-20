using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
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
using Rdd.Web.Querying;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.Serializers;
using Rdd.Web.Serialization.UrlProviders;
using System;

namespace Rdd.Web.Helpers
{
    public static class RddServiceCollectionExtensions
    {
        public static RddBuilder AddRdd<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddOptions<RddOptions>();

            services.TryAddSingleton(typeof(IInstanciator<>), typeof(DefaultInstanciator<>));
            services.TryAddSingleton<IPatcherProvider, PatcherProvider>();
            services.TryAddSingleton(typeof(IPatcher<>), typeof(ObjectPatcher<>));
            services.TryAddSingleton<EnumerablePatcher>();
            services.TryAddSingleton<DictionaryPatcher>();
            services.TryAddSingleton<ValuePatcher>();
            services.TryAddSingleton<DynamicPatcher>();
            services.TryAddSingleton<ObjectPatcher>();

            //serialization
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

            //scoped services
            services.TryAddScoped<DbContext>(p => p.GetRequiredService<TDbContext>());
            services.TryAddScoped<IStorageService, EFStorageService>();
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();

            services.TryAddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
            services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.TryAddScoped(typeof(IReadOnlyRestCollection<,>), typeof(ReadOnlyRestCollection<,>));
            services.TryAddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>));
            services.TryAddScoped(typeof(IReadOnlyAppController<,>), typeof(ReadOnlyAppController<,>));
            services.TryAddScoped(typeof(IAppController<,>), typeof(AppController<,>));
            
            // closed by default, overridable with AddRddDefaultRights
            services.TryAddSingleton(typeof(IRightExpressionsHelper<>), typeof(ClosedRightExpressionsHelper<>));

            return new RddBuilder(services).ApplyRddSetupOptions();
        }

        public static RddBuilder AddRdd<TDbContext>(this IServiceCollection services, Action<RddOptions> onConfigure)
            where TDbContext : DbContext
        {
            var builder = services.AddRdd<TDbContext>();
            services.Configure(onConfigure);
            return builder;
        }

        private static RddBuilder ApplyRddSetupOptions(this RddBuilder rddBuilder)
        {
            rddBuilder.Services.PostConfigure<MvcNewtonsoftJsonOptions>(o =>
            {
                foreach (JsonConverter converter in rddBuilder.JsonConverters)
                {
                    o.SerializerSettings.Converters.Add(converter);
                }
            });
            return rddBuilder;
        }

        /// <summary>
        /// Register Rdd middleware in the pipeline request BEFORE UseMVC
        /// </summary>
        public static IApplicationBuilder UseRdd(this IApplicationBuilder app, RddCompatibilityVersion rddCompatibilityVersion = RddCompatibilityVersion.Version_3_2)
        {

            app.UseMiddleware<EnableRequestRewindMiddleware>();
            if (rddCompatibilityVersion < RddCompatibilityVersion.Version_3_3)
            {
                app.UseMiddleware<HttpStatusCodeExceptionMiddleware>();
            }
            return app;
        }
    }

    public enum RddCompatibilityVersion
    {
        Version_3_2,
        Version_3_3,
    }
}
