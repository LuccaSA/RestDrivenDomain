using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rdd.Domain.Tests.Models;
using System;
using System.Threading.Tasks;

namespace Rdd.Infra.Tests
{
    public class DatabaseTest
    {
        protected virtual DbContext GetContext(string dbName, bool allowClientEvaluation)
        {
            return new DataContext(GetOptions(dbName, allowClientEvaluation));
        }
        protected virtual DbContextOptions<DataContext> GetOptions(string dbName, bool allowClientEvaluation)
        {
            var builder = new DbContextOptionsBuilder<DataContext>()
                .UseSqlServer($@"Server=(localdb)\mssqllocaldb;Database={dbName};Trusted_Connection=True;ConnectRetryCount=0");

            if (!allowClientEvaluation)
            {
                builder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            }

            return builder.Options;
        }

        public async Task RunCodeInsideIsolatedDatabaseAsync(Func<DbContext, Task> code, bool allowClientEvaluation = false)
        {
            using (var context = GetContext(Guid.NewGuid().ToString(), allowClientEvaluation))
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