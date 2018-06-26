using Microsoft.EntityFrameworkCore;
using RDD.Domain.Mocks;
using RDD.Domain.Patchers;
using RDD.Domain.Tests.Models;
using RDD.Infra;
using RDD.Infra.Storage;
using System;

namespace RDD.Domain.Tests.Templates
{
    public class SingleContextTests
    {
        protected IDependencyInjectionResolver _resolver;
        protected Func<string, IStorageService> _newStorage;
        protected IExecutionContext _execution;
        protected ICombinationsHolder _combinationsHolder;
        protected IPatcherProvider _patcherProvider;

        public SingleContextTests()
        {
            _newStorage = name => new EFStorageService(new DataContext(GetOptions(name)));
            _execution = new ExecutionContextMock();
            _combinationsHolder = new CombinationsHolderMock();
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
