using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RDD.Domain;
using RDD.Domain.WebServices;
using RDD.Web.Helpers;
using RDD.Web.Tests.Models;

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
            services.AddDbContext<TestDbContext>(o => {
                    o.UseInMemoryDatabase("Add_writes_to_database");
                });

            services.AddScoped<DbContext>(s => s.GetRequiredService<TestDbContext>());
            
            services.AddRdd();
            services.AddRddRights<CombinationsHolder, CurPrincipal>();

            services.TryAddScoped<IWebServicesCollection, WebServicesCollection>();
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

            app.UseRdd();

            // sample data
            if (dbContext != null)
            {
                FeedTestData(dbContext);
            }

            app.UseMvc(routes =>
            {
                routes.MapRddDefaultRoutes();

                routes.MapRoute(
                    name: "default_route",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Ping", action = "Status" });  
            });
        }

        private static void FeedTestData(DbContext dbContext)
        {
            dbContext.Add(new User
            {
                Id = 1,
                BirthDay = DateTime.Today.AddYears(-42),
                ContractStart =  DateTime.Today.AddYears(-2),
                Department = new Department
                {
                    Id = 1,
                    Name = "Dep"
                },
                Name = "John",
                PictureId = Guid.NewGuid(),
                Salary = 42,
                TwitterUri = new Uri("https://twitter.com/john"),
                MyValueObject = new MyValueObject
                {
                    Id = 1,
                    Name = "abcs"
                },
                Files = new List<UserFile>
                {
                    new UserFile
                    {
                        Id = Guid.NewGuid(),
                        DateUpload = DateTime.Now.AddHours(-24),
                        FileDescriptor = new FileDescriptor
                        {
                            Id = Guid.NewGuid(),
                            FileName = "thefile.jpg"
                        }
                    }
                }
            });
            dbContext.SaveChanges();

            var tmp = dbContext.Set<Department>().ToList();
            var tmp2 = dbContext.Set<User>().ToList();
            ;
        }
    }
}
