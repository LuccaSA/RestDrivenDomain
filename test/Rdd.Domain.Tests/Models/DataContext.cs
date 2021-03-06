﻿using Microsoft.EntityFrameworkCore;

namespace Rdd.Domain.Tests.Models
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
        public DbSet<Department> Departments { get; }
        public DbSet<UserWithParameters> UserWithParameters { get; }

        public DbSet<Parent> Parents { get; }
        public DbSet<OptionalChild> OptionalChildren { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConcreteClassThree>();

            modelBuilder.Entity<AbstractClass>();

            modelBuilder.Entity<User>().Ignore(u => u.TwitterUri);

            modelBuilder.Entity<UserWithParameters>().Ignore(u => u.Mail);
            modelBuilder.Entity<UserWithParameters>().Ignore(u => u.TwitterUri);

            modelBuilder.Entity<Hierarchy>().Ignore(e => e.Type);
            modelBuilder.Entity<Super>();
        }
    }
}
