﻿using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeedApi.Infrastructure.Persistence.Migrations
{
  /// <inheritdoc />
  public partial class CreateUser : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterDatabase()
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.CreateTable(
          name: "Users",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            BirthDate = table.Column<DateOnly>(type: "DATE", nullable: false),
            PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                  .Annotation("MySql:CharSet", "utf8mb4")
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Users", x => x.Id);
          })
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.CreateTable(
          name: "RefreshTokens",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            Token = table.Column<string>(type: "longtext", nullable: false)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            ExpiryTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            UserId = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_RefreshTokens", x => x.Id);
            table.ForeignKey(
                      name: "FK_RefreshTokens_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          })
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.CreateIndex(
          name: "IX_RefreshTokens_UserId",
          table: "RefreshTokens",
          column: "UserId",
          unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "RefreshTokens");

      migrationBuilder.DropTable(
          name: "Users");
    }
  }
}