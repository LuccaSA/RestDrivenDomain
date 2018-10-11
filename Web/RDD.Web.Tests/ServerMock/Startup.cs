using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rdd.Web.Helpers;

namespace Rdd.Web.Tests.ServerMock
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
            services.AddRddSerialization();

            services.AddDbContext<ExchangeRateDbContext>((service, options) => { options.UseInMemoryDatabase(databaseName: "Add_writes_to_database"); });

            services.AddRdd<ExchangeRateDbContext>(Domain.Rights.RightDefaultMode.Open);

            services.AddScoped<ExchangeRateController>();

            services.AddMvc();

            services.AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DbContext dbContext)
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
                dbContext.SaveChanges();
            }

            app.UseRdd();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default_route",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Ping", action = "Status" });
            });
        }
    }
}