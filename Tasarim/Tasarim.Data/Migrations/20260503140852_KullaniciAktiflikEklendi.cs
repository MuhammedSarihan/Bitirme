using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class KullaniciAktiflikEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "Kullanicilar",
                type: "bit",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Kullanicilar",
                keyColumn: "ID",
                keyValue: 1,
                column: "AktifMi",
                value: true);

            migrationBuilder.UpdateData(
                table: "Kullanicilar",
                keyColumn: "ID",
                keyValue: 2,
                column: "AktifMi",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "Kullanicilar");
        }
    }
}
