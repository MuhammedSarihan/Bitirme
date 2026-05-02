using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class UrunOzellikleriTablosuEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UrunOzellikleri",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunID = table.Column<int>(type: "int", nullable: false),
                    AnaKategori = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnaRenk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Materyal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Detaylar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnalizTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunOzellikleri", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UrunOzellikleri_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UrunOzellikleri_UrunID",
                table: "UrunOzellikleri",
                column: "UrunID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UrunOzellikleri");
        }
    }
}
