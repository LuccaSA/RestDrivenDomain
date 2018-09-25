using Microsoft.EntityFrameworkCore;

namespace RDD.Web.Tests.ServerMock
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExchangeRate>();
            modelBuilder.ApplyConfiguration(new UserDbConfiguration());
        }
    }
}