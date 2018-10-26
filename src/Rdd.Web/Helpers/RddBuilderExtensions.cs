using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Web.Models;
using System;
using Rdd.Infra.Storage;

namespace Rdd.Web.Helpers
{
    public static class RddBuilderExtensions
    {
        private static RddBuilder AddJsonConverter(this RddBuilder rddBuilder, JsonConverter jsonConverter)
        {
            rddBuilder.JsonConverters.Add(jsonConverter);
            return rddBuilder;
        }

        public static RddBuilder AddInheritanceConfiguration<TConfig, TEntity, TKey>(this RddBuilder rddBuilder, TConfig config)
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

        public static RddBuilder AddReadOnlyRepository<TRepository, TEntity>(this RddBuilder rddBuilder)
            where TRepository : class, IReadOnlyRepository<TEntity>
            where TEntity : class
        {
            rddBuilder.Services
                .AddScoped<IReadOnlyRepository<TEntity>, TRepository>(s => s.GetRequiredService<TRepository>())
                .AddScoped<TRepository>();

            return rddBuilder;
        }

        public static RddBuilder AddRepository<TRepository, TEntity>(this RddBuilder rddBuilder)
            where TRepository : class, IRepository<TEntity>
            where TEntity : class
        {
            rddBuilder.Services
                .AddScoped<IRepository<TEntity>, TRepository>(s => s.GetRequiredService<TRepository>())
                .AddScoped<IReadOnlyRepository<TEntity>, TRepository>(s => s.GetRequiredService<TRepository>())
                .AddScoped<TRepository>();

            return rddBuilder;
        }

        public static RddBuilder AddReadOnlyRestCollection<TCollection, TEntity, TKey>(this RddBuilder rddBuilder)
            where TCollection : class, IReadOnlyRestCollection<TEntity, TKey>
            where TEntity : class, IEntityBase<TKey>
            where TKey : IEquatable<TKey>
        {
            rddBuilder.Services
                .AddScoped<IReadOnlyRestCollection<TEntity, TKey>, TCollection>(s => s.GetRequiredService<TCollection>())
                .AddScoped<TCollection>();

            return rddBuilder;
        }

        public static RddBuilder AddRestCollection<TCollection, TEntity, TKey>(this RddBuilder rddBuilder)
            where TCollection : class, IRestCollection<TEntity, TKey>
            where TEntity : class, IEntityBase<TKey>
            where TKey : IEquatable<TKey>
        {
            rddBuilder.Services
                .AddScoped<IRestCollection<TEntity, TKey>, TCollection>(s => s.GetRequiredService<TCollection>())
                .AddScoped<IReadOnlyRestCollection<TEntity, TKey>, TCollection>(s => s.GetRequiredService<TCollection>())
                .AddScoped<TCollection>();

            return rddBuilder;
        }

        public static RddBuilder AddReadOnlyAppController<TController, TEntity, TKey>(this RddBuilder rddBuilder)
            where TController : class, IReadOnlyAppController<TEntity, TKey>
            where TEntity : class, IEntityBase<TKey>
            where TKey : IEquatable<TKey>
        {
            rddBuilder.Services
                .AddScoped<IReadOnlyAppController<TEntity, TKey>, TController>(s => s.GetRequiredService<TController>())
                .AddScoped<TController>();

            return rddBuilder;
        }

        public static RddBuilder AddAppController<TController, TEntity, TKey>(this RddBuilder rddBuilder)
            where TController : class, IAppController<TEntity, TKey>
            where TEntity : class, IEntityBase<TKey>
            where TKey : IEquatable<TKey>
        {
            rddBuilder.Services
                .AddScoped<IAppController<TEntity, TKey>, TController>(s => s.GetRequiredService<TController>())
                .AddScoped<IReadOnlyAppController<TEntity, TKey>, TController>(s => s.GetRequiredService<TController>())
                .AddScoped<TController>();

            return rddBuilder;
        }

        public static RddBuilder AddPatcher<TPatcher, T>(this RddBuilder rddBuilder)
            where TPatcher : class, IPatcher<T>
            where T : class
        {
            rddBuilder.Services
                .AddSingleton<IPatcher<T>, TPatcher>(s => s.GetRequiredService<TPatcher>())
                .AddSingleton<TPatcher>();

            return rddBuilder;
        }

        public static RddBuilder AddOnSaveChangesEvent<TOnSaveChanges, T>(this RddBuilder rddBuilder)
            where TOnSaveChanges : class, IOnSaveChangesAsync<T>
            where T : class
        {
            rddBuilder.Services.AddScoped<IUnitOfWork, HookedUnitOfWork>();
            rddBuilder.Services.AddScoped<ISaveEventProcessor, SaveEventProcessor<T>>();
            rddBuilder.Services.AddScoped<IOnSaveChangesAsync<T>, TOnSaveChanges>();
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
    }
}