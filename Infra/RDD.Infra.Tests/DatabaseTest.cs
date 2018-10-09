using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rdd.Domain.Tests.Models;
using System;
using System.Threading.Tasks;

namespace Rdd.Infra.Tests
{
    public class DatabaseTest
    {
        protected virtual DbContext GetContext(string dbName)
        {
            return new DataContext(GetOptions(dbName));
        }
        protected virtual DbContextOptions<DataContext> GetOptions(string dbName)
        {
            return new DbContextOptionsBuilder<DataContext>()
                .UseSqlServer($@"Server=(localdb)\mssqllocaldb;Database={dbName};Trusted_Connection=True;ConnectRetryCount=0")
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options;
        }

        public async Task RunCodeInsideIsolatedDatabaseAsync(Func<DbContext, Task> code)
        {
            using (var context = GetContext(Guid.NewGuid().ToString()))
            {
                try
                {
                    await context.Database.EnsureCreatedAsync();

                    await code(context);
                }
                finally
                {
                    await context.Database.EnsureDeletedAsync();
                }
            }
        }
    }
}
