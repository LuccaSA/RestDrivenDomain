using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RDD.Application;
using RDD.Application.Controllers;
using RDD.Domain;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using RDD.Infra;
using RDD.Infra.Storage;
using RDD.Web.Middleware;
using RDD.Web.Serialization;
using RDD.Web.Serialization.UrlProviders;
using System;
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RDD.Web.Querying;

namespace RDD.Web.Helpers
{
    public static class RddServiceCollectionExtensions
    {
        /// <summary>
        /// Register minimum RDD dependecies. Set up RDD services via Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// IRightsService and IRDDSerialization are missing for this setup to be ready
        /// </summary>
        public static IServiceCollection AddRddCore(this IServiceCollection services)
        {
            // register base services
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

            services.AddHttpContextAccessor();
            services.TryAddScoped<IHttpContextHelper, HttpContextHelper>();

            services.TryAddScoped(typeof(IRightExpressionsHelper<>), typeof(DefaultRightExpressionsHelper<>));

            services.TryAddSingleton<IWebFilterParser, WebFilterParser>();
            services.TryAddSingleton<IPagingParser, PagingParser>();
            services.TryAddSingleton<IHeaderParser, HeaderParser>();
            services.TryAddSingleton<IOrderByParser, OrderByParser>();
            services.TryAddSingleton<IFieldsParser, FieldsParser>();
            services.TryAddSingleton<QueryParsers>();
            services.TryAddSingleton<QueryTokens>();

            services.TryAddScoped<IQueryFactory, QueryFactory>();
            services.TryAddScoped<QueryMetadata>();

            services.TryAddSingleton(typeof(ICandidateFactory<,>), typeof(CandidateFactory<,>));

            services.AddOptions<PagingOptions>();

            return services;
        }

        /// <summary>
        /// Adds custom right management to filter entities
        /// </summary>
        public static IServiceCollection AddRddRights<TCombinationsHolder, TPrincipal>(this IServiceCollection services)
            where TCombinationsHolder : class, ICombinationsHolder
            where TPrincipal : class, IPrincipal
        {
            services.TryAddScoped(typeof(IRightExpressionsHelper<>), typeof(RightExpressionsHelper<>));
            services.TryAddScoped<IPrincipal, TPrincipal>();
            services.TryAddScoped<ICombinationsHolder, TCombinationsHolder>();
            return services;
        }

        /// <summary>
        /// Adds Rdd specific serialisation (fields + metadata)
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRddSerialization(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, RddSerializationSetup>());
            services.TryAddSingleton<IUrlProvider, UrlProvider>();
            services.TryAddSingleton<IPluralizationService>(new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US"))));
            services.Configure<MvcJsonOptions>(jsonOptions =>
            {
                jsonOptions.SerializerSettings.ContractResolver = new SelectiveContractResolver();
            });
            return services;
        }

        public static IServiceCollection AddRdd(this IServiceCollection services)
        {
            return services.AddRddCore()
                .AddRddSerialization();
        }

        /// <summary>
        /// Register RDD middleware in the pipeline request
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRdd(this IApplicationBuilder app)
        {
            SelectiveContractResolver.UrlProvider = app.ApplicationServices.GetRequiredService<IUrlProvider>();
            return app
                .UseMiddleware<QueryContextMiddleware>()
                .UseMiddleware<HttpStatusCodeExceptionMiddleware>();
        }
    }

    public class RddSerializationSetup : IConfigureOptions<MvcOptions>
    {
        private readonly IOptions<MvcJsonOptions> _jsonOptions;
        private readonly ArrayPool<char> _charPool;

        public RddSerializationSetup(IOptions<MvcJsonOptions> jsonOptions, ArrayPool<char> charPool)
        {
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
            _charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
        }

        public void Configure(MvcOptions options)
        {
            options.OutputFormatters.Add(new MetaSelectiveJsonOutputFormatter(_jsonOptions.Value.SerializerSettings, _charPool));
            options.OutputFormatters.Add(new SelectiveJsonOutputFormatter(_jsonOptions.Value.SerializerSettings, _charPool));
        }
    }
}