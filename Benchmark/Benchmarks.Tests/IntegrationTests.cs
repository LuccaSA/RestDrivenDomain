using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models;
using Rdd.Domain.Rights;
using Rdd.Web.Controllers;
using Rdd.Web.Helpers;
using Xunit;

namespace Benchmarks.Tests
{
    public class IntegrationTests
    {
        private readonly HttpClient _client;

        public IntegrationTests()
        {
            var host = Startup.BuildWebHost(null);
            host.ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
            });
            var server = new TestServer(host);
            _client = server.CreateClient();
        }

        [Fact]
        public async Task SimpleGet()
        {
            for (int i = 0; i < 10000; i++)
            {
                await _client.GetAsync("/Orders/");
            }
        }
    }


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
            services.AddRddSerialization<Principal>();

            services.AddDbContext<OrderDbContext>((service, options) => { options.UseInMemoryDatabase("orders"); });
            services.AddRdd<OrderDbContext, CombinationHolder, Principal>();
            services.AddScoped<OrderController>();

            services.AddMvc();

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DbContext dbContext)
        {
            if (dbContext != null)
            {
                var fixture = new Fixture();
                for (int i = 0; i < 42; i++)
                {
                    var order = fixture.Create<OrderEntity>();
                    dbContext.Add(order);
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

   
    public class OrderEntity : EntityBase<OrderEntity, int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
        public OrderLine OrderLines { get; set; }
    }

    public class OrderLine
    {
        public int Id { get; set; }
        public Article Article { get; set; }
        public decimal Quantity { get; set; }
        public decimal VAT { get; set; }
    }

    public class Article
    {
        public int Id { get; set; }
        public String Reference { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public String Color { get; set; }
    }

    [Route("Orders")]
    public class OrderController : WebController<OrderEntity, int>
    {
        public OrderController(IAppController<OrderEntity, int> appController, ApiHelper<OrderEntity, int> helper)
            : base(appController, helper)
        {

        }

        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;
    }

    public class Principal : IPrincipal
    {
        public int Id { get; }
        public string Token { get; set; }
        public string Name { get; }
        public Culture Culture { get; }
        public PrincipalType Type { get; }
    }

    public class CombinationHolder : ICombinationsHolder
    {
        public CombinationHolder()
        {
            Combinations = new[]{new Combination()
            {
                Operation = new Operation(),
                Subject = typeof(OrderEntity),
                Verb = HttpVerbs.All
            }};
        }

        public IEnumerable<Combination> Combinations { get; }
    }

    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions options) : base(options) { }

        public DbSet<OrderEntity> Orders { get; set; }
    }

}
