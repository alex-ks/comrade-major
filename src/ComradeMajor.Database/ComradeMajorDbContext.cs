using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ComradeMajor.Database
{
    public class ComradeMajorDbContext : DbContext
    {
        [MaybeNull]
        public DbSet<User> Users { get; set; }
        [MaybeNull]
        public DbSet<UserAction> UserActions { get; set; }

        private string connectionString_;

        public ComradeMajorDbContext(string connectionString)
        {
            connectionString_ = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            base.OnConfiguring(options);
            options
                .UseLazyLoadingProxies()
                .UseSqlite(connectionString_);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Disable autoincrement on users as user id is Discord-generated.
            builder.Entity<User>().Property(u => u.Id).ValueGeneratedNever();

            // Make DateTimeOffset comparable.
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var properties = from prop in entityType.ClrType.GetProperties()
                                 where prop.PropertyType == typeof(DateTimeOffset) ||
                                       prop.PropertyType == typeof(DateTimeOffset?)
                                 select prop;

                foreach (var property in properties)
                {
                    builder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(new DateTimeOffsetToBinaryConverter());
                }
            }
        }
    }
}
