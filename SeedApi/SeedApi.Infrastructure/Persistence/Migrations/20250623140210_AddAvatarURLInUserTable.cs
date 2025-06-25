using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeedApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarURLInUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarURL",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarURL",
                table: "Users");
        }
    }
}
