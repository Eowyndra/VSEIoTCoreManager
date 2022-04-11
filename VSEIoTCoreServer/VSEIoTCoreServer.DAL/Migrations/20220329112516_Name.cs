﻿// <auto-generated/>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSEIoTCoreServer.DAL.Migrations
{
    public partial class Name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VseIpAddress",
                table: "DeviceConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DeviceConfigurations",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "DeviceConfigurations");

            migrationBuilder.AlterColumn<string>(
                name: "VseIpAddress",
                table: "DeviceConfigurations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
