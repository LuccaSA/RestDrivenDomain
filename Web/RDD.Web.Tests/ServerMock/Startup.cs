using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RDD.Domain;
using RDD.Domain.Patchers;
using RDD.Domain.WebServices;
using RDD.Infra;
using RDD.Infra.Storage;
using RDD.Web.Helpers;
using RDD.Web.Serialization;

namespace RDD.Web.Tests.ServerMock
{
    public class Startup
    {
        public static IWebHostBuilder BuildWebHost(string[] args) => WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            DbContextOptions<ExchangeRateDbContext> options = new DbContextOptionsBuilder<ExchangeRateDbContext>()
                .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
                .Options;

            services.AddScoped<DbContextOptions>(_ => options);

            services.AddRdd<ExchangeRateDbContext, CombinationsHolder, CurPrincipal>();

            services.TryAddScoped<IWebServicesCollection, WebServicesCollection>();
            services.AddScoped<ExchangeRateController>();

            services.AddMvc();

            services.AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStatusCodePages();

            app.UseRdd();

            app.UseMvc(routes =>
            {
                routes.MapRddDefaultRoutes();

                routes.MapRoute(
                    name: "default_route",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Ping", action = "Status" });  
            });
        }
    }
}