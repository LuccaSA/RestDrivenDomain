using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RDD.Infra.Storage;
using System;
using System.Collections.Generic;
using Xunit;

namespace RDD.Infra.Tests
{
    public class DbContextResolverTests
    {
        class User1
        {
            public int Id { get; set; }
        }
        class DbContext1 : DbContext
        {
            DbSet<User1> Users { get; set; }
            public DbContext1(DbContextOptions<DbContext1> options) : base(options)
            {
            }
        }
        class User2
        {
            public int Id { get; set; }
        }
        class DbContext2 : DbContext
        {
            DbSet<User2> Users { get; set; }
            public DbContext2(DbContextOptions<DbContext2> options) : base(options)
            {
            }
        }

        [Fact]
        public void TestMultipleCountexts()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext1>(o => { o.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0"); })
                .AddDbContext<DbContext2>(o => { o.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0"); });

            var provider = serviceCollection.BuildServiceProvider();

            BoundedContextsResolver.DbContextTypes = new List<Type> { typeof(DbContext1), typeof(DbContext2) };
            var resolver = new BoundedContextsResolver(provider);

            var db1 = resolver.GetMatchingContext<User1>();
            var db2 = resolver.GetMatchingContext<User2>();
            var db3 = resolver.GetMatchingContext<User1>();

            Assert.Equal(db1, db3);
            Assert.NotEqual(db1, db2);
        }
    }
}