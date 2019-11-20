using Microsoft.EntityFrameworkCore;
using Rdd.Application;
using Rdd.Domain.Exceptions;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Rdd.Infra.Storage
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;

        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw (ex.InnerException?.InnerException) switch
                {
                    ArgumentException ae => ae,
                    SqlException se => se.Number switch
                    {
                        2627 => new TechnicalException(se.Message),
                        _ => se,
                    },
                    _ => ex.InnerException ?? ex,
                };
            }
        }
    }
}