using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Data.Sqlite;
using Rdd.Domain.Tests.Models;
using System;
using System.Runtime.InteropServices;
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
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new DbContextOptionsBuilder<DataContext>()
                    .UseSqlServer($@"Server=(localdb)\mssqllocaldb;Database={dbName};Trusted_Connection=True;ConnectRetryCount=0")
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options;
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return new DbContextOptionsBuilder<DataContext>()
                    .UseSqlite(connection)
                    // .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options;
            }
            else
            {
                throw new NotImplementedException();
            }
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
