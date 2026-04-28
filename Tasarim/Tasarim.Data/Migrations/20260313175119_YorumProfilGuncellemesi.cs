using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class YorumProfilGuncellemesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Yorumlar_Kullanicilar_KullaniciID",
                table: "Yorumlar");

            migrationBuilder.AlterColumn<int>(
                name: "KullaniciID",
                table: "Yorumlar",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ProfilID",
                table: "Yorumlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "Tarih",
                table: "Yorumlar",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Yorumlar_ProfilID",
                table: "Yorumlar",
                column: "ProfilID");

            migrationBuilder.AddForeignKey(
                name: "FK_Yorumlar_Kullanicilar_KullaniciID",
                table: "Yorumlar",
                column: "KullaniciID",
                principalTable: "Kullanicilar",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Yorumlar_Profiller_ProfilID",
                table: "Yorumlar",
                column: "ProfilID",
                principalTable: "Profiller",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Yorumlar_Kullanicilar_KullaniciID",
                table: "Yorumlar");

            migrationBuilder.DropForeignKey(
                name: "FK_Yorumlar_Profiller_ProfilID",
                table: "Yorumlar");

            migrationBuilder.DropIndex(
                name: "IX_Yorumlar_ProfilID",
                table: "Yorumlar");

            migrationBuilder.DropColumn(
                name: "ProfilID",
                table: "Yorumlar");

            migrationBuilder.DropColumn(
                name: "Tarih",
                table: "Yorumlar");

            migrationBuilder.AlterColumn<int>(
                name: "KullaniciID",
                table: "Yorumlar",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Yorumlar_Kullanicilar_KullaniciID",
                table: "Yorumlar",
                column: "KullaniciID",
                principalTable: "Kullanicilar",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
