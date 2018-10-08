using Microsoft.EntityFrameworkCore;

namespace Rdd.Web.Tests.ServerMock
{
    public class ExchangeRateDbContext : DbContext
    {
        public ExchangeRateDbContext(DbContextOptions options) 
            : base(options)
        {
        }

        public DbSet<ExchangeRate> ExchangeRates { get; set; }

    }
}