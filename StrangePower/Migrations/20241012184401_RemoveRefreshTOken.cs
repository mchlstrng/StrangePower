using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrangePower.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Tokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Tokens",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
