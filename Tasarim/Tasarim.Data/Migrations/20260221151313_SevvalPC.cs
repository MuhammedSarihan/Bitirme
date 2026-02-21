using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class SevvalPC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resimler_Urunler_UrunID",
                table: "Resimler");

            migrationBuilder.AlterColumn<int>(
                name: "UrunID",
                table: "Resimler",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SiraNo",
                table: "Resimler",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Resimler_Urunler_UrunID",
                table: "Resimler",
                column: "UrunID",
                principalTable: "Urunler",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resimler_Urunler_UrunID",
                table: "Resimler");

            migrationBuilder.AlterColumn<int>(
                name: "UrunID",
                table: "Resimler",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SiraNo",
                table: "Resimler",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Resimler_Urunler_UrunID",
                table: "Resimler",
                column: "UrunID",
                principalTable: "Urunler",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
