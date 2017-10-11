using Microsoft.EntityFrameworkCore;

namespace RDD.Domain.Tests.Models
{
    public class DataContext : DbContext
    {
        public DataContext() { }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<ConcreteClassOne> ConcreteClassOne { get; }
        public DbSet<ConcreteClassTwo> ConcreteClassTwo { get; }
        public DbSet<ConcreteClassThree> ConcreteClassThree { get; }
        public DbSet<AbstractClass> AbstractClass { get; }
        public DbSet<User> User { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConcreteClassThree>().Ignore(c => c.AuthorizedActions);
            modelBuilder.Entity<ConcreteClassThree>().Ignore(c => c.AuthorizedOperations);
            modelBuilder.Entity<ConcreteClassThree>().Ignore(c => c.Url);

            modelBuilder.Entity<AbstractClass>().Ignore(a => a.AuthorizedActions);
            modelBuilder.Entity<AbstractClass>().Ignore(a => a.AuthorizedOperations);
            modelBuilder.Entity<AbstractClass>().Ignore(a => a.Url);

            modelBuilder.Entity<User>().Ignore(u => u.AuthorizedActions);
            modelBuilder.Entity<User>().Ignore(u => u.AuthorizedOperations);
            modelBuilder.Entity<User>().Ignore(u => u.Url);
            modelBuilder.Entity<User>().Ignore(u => u.Mail);
            modelBuilder.Entity<User>().Ignore(u => u.TwitterUri);
        }
    }
}
