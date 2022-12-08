﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VSEIoTCoreServer.DAL;

#nullable disable

namespace VSEIoTCoreServer.DAL.Migrations
{
    [DbContext(typeof(SQLiteDbContext))]
    partial class SQLiteDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.1");

            modelBuilder.Entity("VSEIoTCoreServer.DAL.Models.DeviceConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("IoTCorePort")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("VseIpAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("VsePort")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VseType")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("DeviceConfigurations", (string)null);
                });

            modelBuilder.Entity("VSEIoTCoreServer.DAL.Models.GlobalConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GlobalIoTCorePort")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("GlobalConfiguration", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
