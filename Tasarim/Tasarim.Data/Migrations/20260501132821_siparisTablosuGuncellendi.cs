using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class siparisTablosuGuncellendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeslimatAdresi",
                table: "Siparisler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeslimatAdresi",
                table: "Siparisler");
        }
    }
}
