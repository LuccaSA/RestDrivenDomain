using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.UrlProviders;
using System.Globalization;
using System;
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RDD.Domain.Models.Querying;
using RDD.Web.Middleware;
using RDD.Web.Querying;

namespace RDD.Web.Helpers
{
    public static class RddServiceCollectionExtensions
    {
        /// <summary>
        /// Register minimum RDD dependecies. Set up RDD services via Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// IRightsService and IRDDSerialization are missing for this setup to be ready
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddRddCore<TDbContext>(this IServiceCollection services)
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
            services.AddHttpContextAccessor();
            services.TryAddScoped<IHttpContextHelper, HttpContextHelper>();

            services.TryAddSingleton<IWebFilterParser,WebFilterParser> ();
            services.TryAddSingleton<IPagingParser, PagingParser> ();
            services.TryAddSingleton<IHeaderParser, HeaderParser> ();
            services.TryAddSingleton<IOrberByParser, OrberByParser> ();
            services.TryAddSingleton<QueryParsers> ();
            services.TryAddSingleton<QueryTokens>();

            services.TryAddScoped<IQueryFactory, QueryFactory>();
            services.TryAddScoped<QueryMetadata>();

            services.TryAddSingleton(typeof(ICandidateFactory<,>), typeof(CandidateFactory<,>));

            services.AddOptions<RddOptions>();

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
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, RddSerializationSetup>());
            services.TryAddScoped<IUrlProvider, UrlProvider>();
            services.TryAddScoped<IPrincipal, TPrincipal>();
            services.Configure<MvcJsonOptions>(jsonOptions =>
            {
                jsonOptions.SerializerSettings.ContractResolver = new SelectiveContractResolver();
            });
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
        /// Register RDD middleware in the pipeline request
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRdd(this IApplicationBuilder app)
        {
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