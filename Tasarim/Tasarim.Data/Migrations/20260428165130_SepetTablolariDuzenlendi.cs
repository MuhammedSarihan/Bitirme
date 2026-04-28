using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class SepetTablolariDuzenlendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SepetItems_Urunler_UrunID",
                table: "SepetItems");

            migrationBuilder.AlterColumn<int>(
                name: "UrunID",
                table: "SepetItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "UrunVaryasyonID",
                table: "SepetItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SepetItems_UrunVaryasyonID",
                table: "SepetItems",
                column: "UrunVaryasyonID");

            migrationBuilder.AddForeignKey(
                name: "FK_SepetItems_UrunVaryasyonlari_UrunVaryasyonID",
                table: "SepetItems",
                column: "UrunVaryasyonID",
                principalTable: "UrunVaryasyonlari",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SepetItems_Urunler_UrunID",
                table: "SepetItems",
                column: "UrunID",
                principalTable: "Urunler",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SepetItems_UrunVaryasyonlari_UrunVaryasyonID",
                table: "SepetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SepetItems_Urunler_UrunID",
                table: "SepetItems");

            migrationBuilder.DropIndex(
                name: "IX_SepetItems_UrunVaryasyonID",
                table: "SepetItems");

            migrationBuilder.DropColumn(
                name: "UrunVaryasyonID",
                table: "SepetItems");

            migrationBuilder.AlterColumn<int>(
                name: "UrunID",
                table: "SepetItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SepetItems_Urunler_UrunID",
                table: "SepetItems",
                column: "UrunID",
                principalTable: "Urunler",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
