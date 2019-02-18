using Microsoft.EntityFrameworkCore;
using Rdd.Web.Tests.Models;

namespace Rdd.Web.Tests.ServerMock
{
    public class ExchangeRateDbContext : DbContext
    {
        public ExchangeRateDbContext(DbContextOptions options) 
            : base(options)
        {
        }

        public DbSet<Cat> Cats { get; set; }

        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<ExchangeRate2> ExchangeRate2s { get; set; }

    }
}