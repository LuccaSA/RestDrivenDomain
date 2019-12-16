using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            services.AddDbContext<ExchangeRateDbContext>((service, options) =>options.UseInMemoryDatabase("Add_writes_to_database_3_0"));


            services
                .AddRdd<ExchangeRateDbContext>(rdd =>
                {
                    rdd.PagingLimit = 10;
                    rdd.PagingMaximumLimit = 4242;
                })
                .WithDefaultRights(RightDefaultMode.Open)
                .AddAutoMapper(c => c.AddExpressionMapping()
                    .CreateMap<Cat, DTOCat>(MemberList.Destination)
                    .ForMember(dest => dest.NickName, opts => opts.MapFrom(sour => sour.Name))
                    .ForMember(dest => dest.Id, opts => opts.MapFrom(sour => sour.Id))
                    .ForMember(dest => dest.Age, opts => opts.MapFrom(sour => sour.Age))
                    .ReverseMap());

            SetupMvc(services);

            services.AddLogging();
        }

        protected virtual void SetupMvc(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbContext dbContext)
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

            app.UseRouting();
            app.UseEndpoints(b =>
            {
                b.MapControllers();
            });
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
            services.AddControllers();
        }
    }
}