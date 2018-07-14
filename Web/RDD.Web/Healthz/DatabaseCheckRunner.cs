using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace RDD.Web.Healthz
{
    /// <summary>
    /// Checks database connectivity
    /// </summary>
    public class DatabaseCheckRunner : IHealthzCheckRunner
    {
        private readonly DbContext _dbContext;

        public DatabaseCheckRunner(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Dictionary<string, List<HealthzCheck>>> GetStatus()
        {
            return new Dictionary<string, List<HealthzCheck>>
            {
                {"sqlserver", await DatabaseCheck()},
            };
        }

        private async Task<List<HealthzCheck>> DatabaseCheck()
        {
            var checks = new List<HealthzCheck>();
            var dbexists = _dbContext.Database.Exists();

            checks.Add(new HealthzCheck
            {
                ComponentType = "database:exists",
                Status = dbexists ? CheckState.Pass : CheckState.Failed,
            });

            var connectCheck = new HealthzCheck
            {
                ComponentType = "database:connect",
                Status = dbexists ? CheckState.Pass : CheckState.Failed,
            };
            checks.Add(connectCheck);

            try
            {
                // check via ado.net
                int returned;
                var co = _dbContext.Database.GetDbConnection();
                using (var command = co.CreateCommand())
                {
                    command.CommandText = "SELECT 42";
                    await co.OpenAsync();
                    returned = (int)await command.ExecuteScalarAsync();
                }
                connectCheck.Status = returned == 42 ? CheckState.Pass : CheckState.Failed;
            }
            catch (Exception e)
            {
                connectCheck.Status = CheckState.Failed;
                connectCheck.Output = e.ToString();
            }
            return checks;
        }
    }

    internal static class DatabaseFacadeExtensions
    {
        internal static bool Exists(this DatabaseFacade source)
        {
            return source.GetService<IRelationalDatabaseCreator>().Exists();
        }
    }
}