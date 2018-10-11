using Microsoft.EntityFrameworkCore;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System;

namespace Rdd.Domain.Tests.Templates
{
    public class SingleContextTests
    {
        protected Func<string, IStorageService> _newStorage;
        protected IRightExpressionsHelper<User> _rightsService;
        protected IPatcherProvider _patcherProvider;
        protected IInstanciator<User> Instanciator { get; set; }

        public SingleContextTests()
        {
            _newStorage = name => new EFStorageService(new DataContext(GetOptions(name)));
            _rightsService = new OpenRightExpressionsHelper<User>();
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
