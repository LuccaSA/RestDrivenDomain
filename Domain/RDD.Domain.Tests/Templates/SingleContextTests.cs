using Microsoft.EntityFrameworkCore;
using RDD.Domain.Mocks;
using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using RDD.Domain.Tests.Models;
using RDD.Infra;
using RDD.Infra.Storage;
using System;
using RDD.Domain.Models.Querying;

namespace RDD.Domain.Tests.Templates
{
    public class SingleContextTests
    {
        protected Func<string, IStorageService> _newStorage;
        protected IRightExpressionsHelper<User> _rightsService;
        protected IPatcherProvider _patcherProvider;
        protected IInstanciator<User> Instanciator { get; }
        protected QueryContext QueryContex { get; }

        public SingleContextTests()
        {
            QueryContex = new QueryContext(new QueryRequest(), new QueryResponse());
            _newStorage = name => new EFStorageService(new DataContext(GetOptions(name)));
            _rightsService = new RightsServiceMock<User>();
            _patcherProvider = new PatcherProvider();
            Instanciator = new DefaultInstanciator<User>();
        }

        private DbContextOptions<DataContext> GetOptions(string name)
        {
            return new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: name)
                //                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;
        }
    }
}
