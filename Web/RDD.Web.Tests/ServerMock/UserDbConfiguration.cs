using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RDD.Web.Tests.Models;

namespace RDD.Web.Tests.ServerMock
{
    public class UserDbConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.TwitterUri).HasConversion(v => v.ToString(), v => new Uri(v));
        }
    }
}