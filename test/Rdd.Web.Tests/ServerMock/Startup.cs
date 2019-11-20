using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#if NETCOREAPP3_0
using Microsoft.Extensions.Hosting;
#endif
using Rdd.Domain.Rights;
using Rdd.Web.AutoMapper;
using Rdd.Web.Helpers;
using Rdd.Web.Tests.Models;

namespace Rdd.Web.Tests.ServerMock
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ExchangeRateDbContext>((service, options) =>
#if NETCOREAPP2_2
                options.UseInMemoryDatabase("Add_writes_to_database_2_2"));
#endif
#if NETCOREAPP3_0
                options.UseInMemoryDatabase("Add_writes_to_database_3_0"));
#endif

            services
                .AddRdd<ExchangeRateDbContext>(rdd =>
                {
                    rdd.PagingLimit = 10;
                    rdd.PagingMaximumLimit = 4242;
                })
                .WithDefaultRights(RightDefaultMode.Open)
                .AddAutoMapper();

            SetupMvc(services);

            services.AddLogging();
        }

        protected virtual void SetupMvc(IServiceCollection services)
        {
            services.AddMvc();
        }

#if NETCOREAPP2_2
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DbContext dbContext)
#endif
#if NETCOREAPP3_0
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbContext dbContext)
#endif
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStatusCodePages();

            if (dbContext != null)
            {
                for (int i = 0; i < 42; i++)
                {
                    dbContext.Add(new ExchangeRate
                    {
                        Name = i.ToString()
                    });
                }

                dbContext.Add(new Cat
                {
                    Name = "kitty",
                    Age = 22
                });
                dbContext.Add(new Cat
                {
                    Name = "kitty",
                    Age = 23
                });

                dbContext.SaveChanges();
            }

            app.UseRdd();

#if NETCOREAPP2_2
            app.UseMvc(routes => routes.MapRoute(
                name: "default_route",
                template: "{controller}/{action}/{id?}",
                defaults: new { controller = "Ping", action = "Status" }));
#endif
#if NETCOREAPP3_0
            app.UseRouting();
            app.UseEndpoints(b =>
            {
                b.MapControllers();
            });
#endif
        }
    }

    public static class HostBuilder
    {
        public static IWebHostBuilder FromStartup<TStartup>()
            where TStartup : Startup
            => FromStartup<TStartup>(null);

        public static IWebHostBuilder FromStartup<TStartup>(string[] args)
            where TStartup : Startup
            => WebHost.CreateDefaultBuilder(args).UseStartup<TStartup>();
    }

    public class StartupMvc22 : Startup
    {
        public StartupMvc22(IConfiguration configuration) : base(configuration) { }

        protected override void SetupMvc(IServiceCollection services)
        {
            services
                .AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Latest);
        }
    }
}