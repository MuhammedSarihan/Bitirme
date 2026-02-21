using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class UrunVaryasyonEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Beden",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "Renk",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "Stok",
                table: "Urunler");

            migrationBuilder.AlterColumn<int>(
                name: "SiraNo",
                table: "Kategoriler",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "UrunVaryasyonlari",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Beden = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Renk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StokAdedi = table.Column<int>(type: "int", nullable: false),
                    UrunID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunVaryasyonlari", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UrunVaryasyonlari_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_UstKategoriID",
                table: "Kategoriler",
                column: "UstKategoriID");

            migrationBuilder.CreateIndex(
                name: "IX_UrunVaryasyonlari_UrunID",
                table: "UrunVaryasyonlari",
                column: "UrunID");

            migrationBuilder.AddForeignKey(
                name: "FK_Kategoriler_Kategoriler_UstKategoriID",
                table: "Kategoriler",
                column: "UstKategoriID",
                principalTable: "Kategoriler",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kategoriler_Kategoriler_UstKategoriID",
                table: "Kategoriler");

            migrationBuilder.DropTable(
                name: "UrunVaryasyonlari");

            migrationBuilder.DropIndex(
                name: "IX_Kategoriler_UstKategoriID",
                table: "Kategoriler");

            migrationBuilder.AddColumn<string>(
                name: "Beden",
                table: "Urunler",
                type: "varchar(5)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Renk",
                table: "Urunler",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Stok",
                table: "Urunler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SiraNo",
                table: "Kategoriler",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
