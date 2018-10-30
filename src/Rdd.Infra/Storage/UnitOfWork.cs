using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rdd.Application;
using Rdd.Infra.Exceptions;

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
                switch (ex.InnerException?.InnerException)
                {
                    case ArgumentException ae:
                        throw ae;
                    case SqlException se:
                        switch (se.Number)
                        {
                            case 2627:
                                throw new SqlUniqConstraintException(se.Message);
                            default:
                                throw se;
                        }
                    default:
                        throw ex.InnerException ?? ex;
                }
            }
        }
    }
}