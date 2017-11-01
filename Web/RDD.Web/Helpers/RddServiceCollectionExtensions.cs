using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using RDD.Application;
using RDD.Application.Controllers;
using RDD.Domain;
using RDD.Domain.Models;
using RDD.Infra;
using RDD.Infra.Contexts;
using RDD.Infra.Storage;
using RDD.Web.Serialization;

namespace RDD.Web.Helpers
{
    /// <summary>
    ///  Extension methods for setting up RDD services in an Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// </summary>
    public static class RddServiceCollectionExtensions
    {
        public static void AddRdd(this IServiceCollection services)
        {
            services.AddScoped<IWebContextWrapper, HttpContextWrapper>();
            services.AddScoped<IWebContext, HttpContextWrapper>();
            services.AddScoped(typeof(ApiHelper<,>));
            services.AddSingleton<IContractResolver, CamelCasePropertyNamesContractResolver>();
            services.AddScoped<IEntitySerializer, EntitySerializer>();
            services.AddScoped(typeof(IAppController<,>), typeof(AppController<,>));
            services.AddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }
    }
}