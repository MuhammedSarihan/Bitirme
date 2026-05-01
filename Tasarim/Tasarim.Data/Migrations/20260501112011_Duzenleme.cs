using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class Duzenleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TopOneriler",
                table: "LLSonuclari",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TopSikayetler",
                table: "LLSonuclari",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TopOneriler",
                table: "LLSonuclari");

            migrationBuilder.DropColumn(
                name: "TopSikayetler",
                table: "LLSonuclari");
        }
    }
}
