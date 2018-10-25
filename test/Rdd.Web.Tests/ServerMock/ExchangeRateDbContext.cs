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
        public DbSet<ExchangeRate2> ExchangeRate2s { get; set; }

    }
}