using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rdd.Domain.Tests.Models;
using System;
using System.Threading.Tasks;

namespace Rdd.Infra.Tests
{
    public class DatabaseTest
    {
        protected string DbName { get; private set; }
        protected virtual DbContext GetContext(bool allowClientEvaluation)
        {
            return new DataContext(GetOptions(allowClientEvaluation));
        }
        protected virtual DbContextOptions<DataContext> GetOptions(bool allowClientEvaluation)
        {
            var builder = new DbContextOptionsBuilder<DataContext>()
                .UseSqlServer($@"Server=(localdb)\mssqllocaldb;Database={DbName};Trusted_Connection=True;ConnectRetryCount=0");

            if (!allowClientEvaluation)
            {
                builder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            }

            return builder.Options;
        }

        public async Task RunCodeInsideIsolatedDatabaseAsync(Func<DbContext, Task> code, bool allowClientEvaluation = false)
        {
            DbName = Guid.NewGuid().ToString();

            using (var context = GetContext(allowClientEvaluation))
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