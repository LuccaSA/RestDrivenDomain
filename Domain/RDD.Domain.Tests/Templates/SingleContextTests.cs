using Microsoft.EntityFrameworkCore;
using RDD.Domain.Mocks;
using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using RDD.Domain.Tests.Models;
using RDD.Infra;
using RDD.Infra.Storage;
using System;

namespace RDD.Domain.Tests.Templates
{
    public class DbContextResolverMock : IDbContextResolver
    {
        DbContext dbContext;

        public DbContextResolverMock(DbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public DbContext GetMatchingContext<TEntity>() => dbContext;
    }

    public class SingleContextTests
    {
        protected Func<string, IStorageService<User>> _newStorage;
        protected IRightExpressionsHelper _rightsService;
        protected IPatcherProvider _patcherProvider;
        protected IInstanciator<User> Instanciator { get; set; }

        public SingleContextTests()
        {
            _newStorage = name => new EFStorageService<User>(new DbContextResolverMock(new DataContext(GetOptions(name))));
            _rightsService = new RightsServiceMock();
            _patcherProvider = new PatcherProvider();
            Instanciator = new DefaultInstanciator<User>();
        }

        protected DbContextOptions<DataContext> GetOptions(string name)
        {
            return new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: name)
                //                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;
        }
    }
}
