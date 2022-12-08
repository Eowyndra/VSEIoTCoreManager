// ----------------------------------------------------------------------------
// Filename: SQLiteDbContext.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.DAL
{
    using Microsoft.EntityFrameworkCore;
    using VSEIoTCoreServer.DAL.Models;

    public class SQLiteDbContext : DbContext
    {
        public SQLiteDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public SQLiteDbContext()
        {
        }

        public virtual DbSet<DeviceConfiguration> DeviceConfigurations { get; set; }
        public virtual DbSet<GlobalConfiguration> GlobalConfiguration { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<DeviceConfiguration>().ToTable("DeviceConfigurations");
            modelBuilder.Entity<DeviceConfiguration>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.HasIndex(i => i.Id).IsUnique();
            });

            modelBuilder.Entity<GlobalConfiguration>().ToTable("GlobalConfiguration");
            modelBuilder.Entity<GlobalConfiguration>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.HasIndex(i => i.Id).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}