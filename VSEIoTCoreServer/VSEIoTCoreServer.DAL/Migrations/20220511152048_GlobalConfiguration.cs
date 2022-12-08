﻿// <auto-generated/>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSEIoTCoreServer.DAL.Migrations
{
    public partial class GlobalConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "GlobalConfiguration",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GlobalConfiguration",
                table: "GlobalConfiguration",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalConfiguration_Id",
                table: "GlobalConfiguration",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GlobalConfiguration",
                table: "GlobalConfiguration");

            migrationBuilder.DropIndex(
                name: "IX_GlobalConfiguration_Id",
                table: "GlobalConfiguration");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "GlobalConfiguration");
        }
    }
}