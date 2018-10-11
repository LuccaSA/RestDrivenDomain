using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class RddBuilder
    {
        public IServiceCollection Services { get; private set; }

        public RightDefaultMode Mode { get; set; }
        public NamingStrategy NamingStrategy { get; set; }

        private readonly List<JsonConverter> _inheritanceJsonConverters;

        public RddBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            _inheritanceJsonConverters = new List<JsonConverter>();

            //Default values
            Mode = RightDefaultMode.Closed;
            NamingStrategy = new CamelCaseNamingStrategy();
        }

        public IServiceCollection Build<TDbContext>()
            where TDbContext : DbContext
        {
            Services.TryAddScoped<DbContext>(p => p.GetService<TDbContext>());

            Services.TryAddScoped<EFStorageService>();
            Services.TryAddScoped<IStorageService>(s => s.GetService<EFStorageService>());

            switch (Mode)
            {
                case RightDefaultMode.Closed:
                    Services.TryAddSingleton(typeof(IRightExpressionsHelper<>), typeof(ClosedRightExpressionsHelper<>));
                    break;
                case RightDefaultMode.Open:
                    Services.TryAddSingleton(typeof(IRightExpressionsHelper<>), typeof(OpenRightExpressionsHelper<>));
                    break;
            }

            Services.TryAddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
            Services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
            Services.TryAddScoped(typeof(IReadOnlyRestCollection<,>), typeof(ReadOnlyRestCollection<,>));
            Services.TryAddScoped(typeof(IReadOnlyAppController<,>), typeof(ReadOnlyAppController<,>));

            Services.TryAddSingleton<IPatcherProvider, PatcherProvider>();
            Services.TryAddSingleton(typeof(IPatcher<>), typeof(ObjectPatcher<>));
            Services.TryAddSingleton(typeof(IInstanciator<>), typeof(DefaultInstanciator<>));
            Services.TryAddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>));

            Services.TryAddScoped<IUnitOfWork>(s => s.GetService<EFStorageService>());
            Services.TryAddScoped(typeof(IAppController<,>), typeof(AppController<,>));

            Services.AddHttpContextAccessor();
            Services.TryAddScoped<IHttpContextHelper, HttpContextHelper>();
            Services.TryAddScoped(typeof(ApiHelper<,>));

            //Serialization
            Services.TryAddSingleton(typeof(Inflector.Inflector), p => new Inflector.Inflector(new CultureInfo("en-US")));
            Services.TryAddSingleton<IPluralizationService, PluralizationService>();

            Services.TryAddSingleton<IUrlProvider, UrlProvider>();

            Services.AddMemoryCache();
            Services.TryAddSingleton<IReflectionProvider, ReflectionProvider>();

            Services.TryAddSingleton(NamingStrategy);
            Services.TryAddSingleton<ISerializerProvider, SerializerProvider>();

            Services.TryAddSingleton<ArraySerializer>();
            Services.TryAddSingleton<CultureInfoSerializer>();
            Services.TryAddSingleton<DateTimeSerializer>();
            Services.TryAddSingleton<DictionarySerializer>();
            Services.TryAddSingleton<MetadataSerializer>();
            Services.TryAddSingleton<SelectionSerializer>();
            Services.TryAddSingleton<ToStringSerializer>();
            Services.TryAddSingleton<ValueSerializer>();

            //used in serialization
            Services.TryAddTransient(s => s.GetService<IHttpContextAccessor>().HttpContext.User);

            if (_inheritanceJsonConverters.Count != 0)
            {
                Services.Configure<MvcJsonOptions>(o => 
                {
                    o.SerializerSettings.Converters = o.SerializerSettings.Converters ?? new List<JsonConverter>();
                    foreach (var converter in _inheritanceJsonConverters)
                    {
                        o.SerializerSettings.Converters.Add(converter);
                    }
                });
            }

            return Services;
        }

        public RddBuilder SetDefaultRights(RightDefaultMode mode)
        {
            Mode = mode;
            return this;
        }

        public RddBuilder SetSerializationNamingStrategy(NamingStrategy strategy)
        {
            NamingStrategy = strategy;
            return this;
        }

        public RddBuilder AddInheritance<TConfig, TEntity, TKey>(TConfig config)
            where TConfig : class, IInheritanceConfiguration<TEntity>
            where TEntity : class, IEntityBase<TEntity, TKey>
            where TKey : IEquatable<TKey>
        {
            Services.AddSingleton<IInheritanceConfiguration>(s => config);
            Services.AddSingleton<IInheritanceConfiguration<TEntity>>(s => config);
            Services.TryAddSingleton<IPatcher<TEntity>, BaseClassPatcher<TEntity>>();
            Services.TryAddSingleton<IInstanciator<TEntity>, BaseClassInstanciator<TEntity>>();

            _inheritanceJsonConverters.Add(new BaseClassJsonConverter<TEntity>(config));

            return this;
        }
    }
}