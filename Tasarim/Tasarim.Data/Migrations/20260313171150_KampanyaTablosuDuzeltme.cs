using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class KampanyaTablosuDuzeltme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<int>(
            //    name: "IndirimTipi",
            //    table: "Kampanyalar",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<decimal>(
            //    name: "IndirimTutari",
            //    table: "Kampanyalar",
            //    type: "decimal(18,2)",
            //    nullable: false,
            //    defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "İletisimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Konu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mesaj = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MesajTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OkunduMu = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_İletisimler", x => x.Id);
                });

            //    migrationBuilder.CreateTable(
            //        name: "KampanyaUrunleri",
            //        columns: table => new
            //        {
            //            UrunID = table.Column<int>(type: "int", nullable: false),
            //            KampanyaID = table.Column<int>(type: "int", nullable: false)
            //        },
            //        constraints: table =>
            //        {
            //            table.PrimaryKey("PK_KampanyaUrunleri", x => new { x.UrunID, x.KampanyaID });
            //            table.ForeignKey(
            //                name: "FK_KampanyaUrunleri_Kampanyalar_KampanyaID",
            //                column: x => x.KampanyaID,
            //                principalTable: "Kampanyalar",
            //                principalColumn: "ID",
            //                onDelete: ReferentialAction.Cascade);
            //            table.ForeignKey(
            //                name: "FK_KampanyaUrunleri_Urunler_UrunID",
            //                column: x => x.UrunID,
            //                principalTable: "Urunler",
            //                principalColumn: "ID",
            //                onDelete: ReferentialAction.Cascade);
            //        });

            //    migrationBuilder.CreateIndex(
            //        name: "IX_KampanyaUrunleri_KampanyaID",
            //        table: "KampanyaUrunleri",
            //        column: "KampanyaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "İletisimler");

            //migrationBuilder.DropTable(
            //    name: "KampanyaUrunleri");

            //migrationBuilder.DropColumn(
            //    name: "IndirimTipi",
            //    table: "Kampanyalar");

            //migrationBuilder.DropColumn(
            //    name: "IndirimTutari",
            //    table: "Kampanyalar");
        }
    }
}
