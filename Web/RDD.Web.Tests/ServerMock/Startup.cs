using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RDD.Domain;
using RDD.Domain.Patchers;
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
            services.AddScoped<DbContext, ExchangeRateDbContext>();

            // register RDD 
            services.AddRdd();
            services.AddRddRights<CombinationsHolder>(p => new CurPrincipal());

            services.AddSingleton<IUrlProvider, UrlProvider>();
            services.AddScoped<IStorageService, EFStorageService>();
            services.AddScoped<IPatcherProvider, PatcherProvider>();
            services.AddScoped<IHttpContextHelper, HttpContextHelper>();

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