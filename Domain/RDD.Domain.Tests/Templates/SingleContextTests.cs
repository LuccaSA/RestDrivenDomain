using Microsoft.EntityFrameworkCore;
using RDD.Domain.Mocks;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using RDD.Domain.Tests.Models;
using RDD.Infra;
using RDD.Infra.Storage;
using System;

namespace RDD.Domain.Tests.Templates
{
    public class SingleContextTests
    {
        protected Func<string, IStorageService> _newStorage;
        protected IRightExpressionsHelper _rightsService;
        protected IPatcherProvider _patcherProvider;

        public SingleContextTests()
        {
            _newStorage = name => new EFStorageService(new DataContext(GetOptions(name)));
            _rightsService = new RightsServiceMock();
            _patcherProvider = new PatcherProvider();
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
