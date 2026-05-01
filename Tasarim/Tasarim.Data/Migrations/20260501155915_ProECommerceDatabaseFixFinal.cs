using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProECommerceDatabaseFixFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetaylari_Urunler_UrunID",
                table: "SiparisDetaylari");

            migrationBuilder.AddColumn<int>(
                name: "UrunVaryasyonID",
                table: "SiparisDetaylari",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Siparisler_SiparisNo",
                table: "Siparisler",
                column: "SiparisNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiparisDetaylari_UrunVaryasyonID",
                table: "SiparisDetaylari",
                column: "UrunVaryasyonID");

            migrationBuilder.CreateIndex(
                name: "IX_Profiller_Mail",
                table: "Profiller",
                column: "Mail",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetaylari_UrunVaryasyonlari_UrunVaryasyonID",
                table: "SiparisDetaylari",
                column: "UrunVaryasyonID",
                principalTable: "UrunVaryasyonlari",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetaylari_Urunler_UrunID",
                table: "SiparisDetaylari",
                column: "UrunID",
                principalTable: "Urunler",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetaylari_UrunVaryasyonlari_UrunVaryasyonID",
                table: "SiparisDetaylari");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetaylari_Urunler_UrunID",
                table: "SiparisDetaylari");

            migrationBuilder.DropIndex(
                name: "IX_Siparisler_SiparisNo",
                table: "Siparisler");

            migrationBuilder.DropIndex(
                name: "IX_SiparisDetaylari_UrunVaryasyonID",
                table: "SiparisDetaylari");

            migrationBuilder.DropIndex(
                name: "IX_Profiller_Mail",
                table: "Profiller");

            migrationBuilder.DropColumn(
                name: "UrunVaryasyonID",
                table: "SiparisDetaylari");

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetaylari_Urunler_UrunID",
                table: "SiparisDetaylari",
                column: "UrunID",
                principalTable: "Urunler",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
