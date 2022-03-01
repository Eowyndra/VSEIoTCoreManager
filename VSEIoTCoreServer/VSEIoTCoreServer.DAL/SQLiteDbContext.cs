using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VSEIoTCoreServer.DAL.Models;

namespace VSEIoTCoreServer.DAL
{
    public class SQLiteDbContext : DbContext
    {
        public SQLiteDbContext(DbContextOptions options) : base(options)
        {

        }

        public SQLiteDbContext()
        {

        }

        public virtual DbSet<DeviceConfiguration> DeviceConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeviceConfiguration>().ToTable("DeviceConfigurations");
            modelBuilder.Entity<DeviceConfiguration>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.HasIndex(i => i.Id).IsUnique();
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}