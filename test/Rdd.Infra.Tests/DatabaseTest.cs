using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Rdd.Domain.Tests.Models;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Rdd.Infra.Tests
{
    public class DatabaseTest
    {
        protected string DbName { get; private set; }
        protected virtual DbContext GetContext()
        {
            return new DataContext(GetOptions());
        }
        protected virtual DbContextOptions<DataContext> GetOptions()
        {
            DbContextOptionsBuilder<DataContext> builder;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder = new DbContextOptionsBuilder<DataContext>()
                    .UseSqlServer($@"Server=(localdb)\mssqllocaldb;Database={DbName};Trusted_Connection=True;ConnectRetryCount=0");
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                builder = new DbContextOptionsBuilder<DataContext>()
                    .UseSqlite(connection);
            }
            else
            {
                throw new NotImplementedException();
            }

            return builder.Options;
        }

        public async Task RunCodeInsideIsolatedDatabaseAsync(Func<DbContext, Task> code)
        {
            DbName = Guid.NewGuid().ToString();

            using (var context = GetContext())
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