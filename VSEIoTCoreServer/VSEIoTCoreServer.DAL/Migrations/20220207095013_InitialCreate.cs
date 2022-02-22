﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSEIoTCoreServer.DAL.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VseType = table.Column<string>(type: "TEXT", nullable: true),
                    VseIpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    VsePort = table.Column<int>(type: "INTEGER", nullable: false),
                    IoTCorePort = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceConfigurations_Id",
                table: "DeviceConfigurations",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceConfigurations");
        }
    }
}
