using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class UrunUrunVaryasyonGncllendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Renk",
                table: "UrunVaryasyonlari");

            migrationBuilder.AddColumn<string>(
                name: "ModelKodu",
                table: "Urunler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Renk",
                table: "Urunler",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelKodu",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "Renk",
                table: "Urunler");

            migrationBuilder.AddColumn<string>(
                name: "Renk",
                table: "UrunVaryasyonlari",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
