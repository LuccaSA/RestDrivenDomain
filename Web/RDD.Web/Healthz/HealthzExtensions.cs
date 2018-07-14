using System;
using Microsoft.Extensions.DependencyInjection;

namespace RDD.Web.Healthz
{
    public static class HealthzExtensions
    {
        /// <summary>
        /// Adds /healthz and /ping json endpoints
        /// </summary>
        /// <param name="services">IServiceCollection from ConfigureServices</param>
        /// <param name="healthConfigure">Requiered Health options</param>
        /// <returns></returns>
        public static IServiceCollection AddHealthCheck(this IServiceCollection services, Action<HealthzOptions> healthConfigure)
        {
            services.AddScoped<HealthzController>();

            services.Configure(healthConfigure);

            var dt = UpTime.StartDateTime; // initialize application start time

            services.AddScoped<HealthzReportService>();
            services.AddScoped<IHealthzCheckRunner, SystemHealthzCheckRunner>();
            services.AddScoped<IHealthzCheckRunner, DatabaseCheckRunner>();
            return services;
        }
    }
}