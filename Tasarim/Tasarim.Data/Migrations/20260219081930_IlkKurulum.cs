using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tasarim.Data.Migrations
{
    /// <inheritdoc />
    public partial class IlkKurulum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kampanyalar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KampanyaAd = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    KampanyaResmi = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    KampanyaAktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kampanyalar", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KategoriAd = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    KategoriResmi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiraNo = table.Column<int>(type: "int", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    UstKategoriID = table.Column<int>(type: "int", nullable: true),
                    UstteGoster = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAd = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Sifre = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    AdminMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Markalar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkaAd = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Logo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markalar", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SiparisDurumlari",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DurumAd = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiparisDurumlari", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Sliders",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    SliderResim = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    SliderAciklama = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    Link = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    SiraNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sliders", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Profiller",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Soyad = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Mail = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    TelNo = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    Adres = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true),
                    Cinsiyet = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    Yas = table.Column<int>(type: "int", nullable: true),
                    Boy = table.Column<double>(type: "float", nullable: true),
                    Kilo = table.Column<double>(type: "float", nullable: true),
                    KullaniciID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiller", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Profiller_Kullanicilar_KullaniciID",
                        column: x => x.KullaniciID,
                        principalTable: "Kullanicilar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sepetler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sepetler", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Sepetler_Kullanicilar_KullaniciID",
                        column: x => x.KullaniciID,
                        principalTable: "Kullanicilar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Urunler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunKod = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Baslik = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Fiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stok = table.Column<int>(type: "int", nullable: false),
                    Renk = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Beden = table.Column<string>(type: "varchar(5)", maxLength: 10, nullable: false),
                    AnaResim = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    KategoriID = table.Column<int>(type: "int", nullable: false),
                    MarkaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Urunler", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Urunler_Kategoriler_KategoriID",
                        column: x => x.KategoriID,
                        principalTable: "Kategoriler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Urunler_Markalar_MarkaID",
                        column: x => x.MarkaID,
                        principalTable: "Markalar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Siparisler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiparisNo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    SiparisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KargoNo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    KullaniciID = table.Column<int>(type: "int", nullable: false),
                    SiparisDurumuID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Siparisler", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Siparisler_Kullanicilar_KullaniciID",
                        column: x => x.KullaniciID,
                        principalTable: "Kullanicilar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Siparisler_SiparisDurumlari_SiparisDurumuID",
                        column: x => x.SiparisDurumuID,
                        principalTable: "SiparisDurumlari",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favoriler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciID = table.Column<int>(type: "int", nullable: false),
                    UrunID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoriler", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Favoriler_Kullanicilar_KullaniciID",
                        column: x => x.KullaniciID,
                        principalTable: "Kullanicilar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoriler_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LLSonuclari",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToplamYorum = table.Column<int>(type: "int", nullable: false),
                    DuyguDagilim = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopArtilar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopEksiler = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SonGuncelleme = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UrunID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LLSonuclari", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LLSonuclari_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resimler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResimYolu = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    SiraNo = table.Column<int>(type: "int", nullable: false),
                    UrunID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resimler", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Resimler_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SepetItems",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adet = table.Column<int>(type: "int", nullable: false),
                    SepetID = table.Column<int>(type: "int", nullable: false),
                    UrunID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SepetItems", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SepetItems_Sepetler_SepetID",
                        column: x => x.SepetID,
                        principalTable: "Sepetler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SepetItems_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Yorumlar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Puan = table.Column<int>(type: "int", nullable: false),
                    YorumIcerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KullaniciID = table.Column<int>(type: "int", nullable: false),
                    UrunID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Yorumlar", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Yorumlar_Kullanicilar_KullaniciID",
                        column: x => x.KullaniciID,
                        principalTable: "Kullanicilar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Yorumlar_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Odemeler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IslemNo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OdemeDurumu = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    SiparisID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Odemeler", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Odemeler_Siparisler_SiparisID",
                        column: x => x.SiparisID,
                        principalTable: "Siparisler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiparisDetaylari",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiparisID = table.Column<int>(type: "int", nullable: false),
                    UrunID = table.Column<int>(type: "int", nullable: false),
                    Adet = table.Column<int>(type: "int", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiparisDetaylari", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SiparisDetaylari_Siparisler_SiparisID",
                        column: x => x.SiparisID,
                        principalTable: "Siparisler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiparisDetaylari_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YorumAnalizleri",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Duygu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Artilar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Eksiler = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sikayetler = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Oneriler = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YorumID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YorumAnalizleri", x => x.ID);
                    table.ForeignKey(
                        name: "FK_YorumAnalizleri_Yorumlar_YorumID",
                        column: x => x.YorumID,
                        principalTable: "Yorumlar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Kategoriler",
                columns: new[] { "ID", "AktifMi", "KategoriAd", "KategoriResmi", "SiraNo", "UstKategoriID", "UstteGoster" },
                values: new object[,]
                {
                    { 1, true, "Erkek", null, 1, null, true },
                    { 2, true, "Kadin", null, 2, null, true },
                    { 3, true, "Elektronik", null, 3, null, false }
                });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "ID", "AdminMi", "KullaniciAd", "Sifre" },
                values: new object[,]
                {
                    { 1, true, "Admin", "1" },
                    { 2, false, "Kullanıcı", "1" }
                });

            migrationBuilder.InsertData(
                table: "Markalar",
                columns: new[] { "ID", "AktifMi", "Logo", "MarkaAd" },
                values: new object[,]
                {
                    { 1, true, null, "Beyoğlu Abiye" },
                    { 2, true, null, "Tuay" },
                    { 3, true, null, "Hennin" }
                });

            migrationBuilder.InsertData(
                table: "SiparisDurumlari",
                columns: new[] { "ID", "DurumAd" },
                values: new object[,]
                {
                    { 1, "Onay Bekliyor" },
                    { 2, "Hazırlanıyor" },
                    { 3, "Kargolandı" },
                    { 4, "Teslim Edildi" }
                });

            migrationBuilder.InsertData(
                table: "Profiller",
                columns: new[] { "ID", "Ad", "Adres", "Boy", "Cinsiyet", "Kilo", "KullaniciID", "Mail", "Soyad", "TelNo", "Yas" },
                values: new object[,]
                {
                    { 1, "Admin", "Üsküdar", null, null, null, 1, "info@admin.com", "1", null, null },
                    { 2, "Kullanıcı", "Ümraniye", null, null, null, 2, "info@kullanıcı.com", "1", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favoriler_KullaniciID",
                table: "Favoriler",
                column: "KullaniciID");

            migrationBuilder.CreateIndex(
                name: "IX_Favoriler_UrunID",
                table: "Favoriler",
                column: "UrunID");

            migrationBuilder.CreateIndex(
                name: "IX_LLSonuclari_UrunID",
                table: "LLSonuclari",
                column: "UrunID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Odemeler_SiparisID",
                table: "Odemeler",
                column: "SiparisID");

            migrationBuilder.CreateIndex(
                name: "IX_Profiller_KullaniciID",
                table: "Profiller",
                column: "KullaniciID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resimler_UrunID",
                table: "Resimler",
                column: "UrunID");

            migrationBuilder.CreateIndex(
                name: "IX_SepetItems_SepetID",
                table: "SepetItems",
                column: "SepetID");

            migrationBuilder.CreateIndex(
                name: "IX_SepetItems_UrunID",
                table: "SepetItems",
                column: "UrunID");

            migrationBuilder.CreateIndex(
                name: "IX_Sepetler_KullaniciID",
                table: "Sepetler",
                column: "KullaniciID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiparisDetaylari_SiparisID",
                table: "SiparisDetaylari",
                column: "SiparisID");

            migrationBuilder.CreateIndex(
                name: "IX_SiparisDetaylari_UrunID",
                table: "SiparisDetaylari",
                column: "UrunID");

            migrationBuilder.CreateIndex(
                name: "IX_Siparisler_KullaniciID",
                table: "Siparisler",
                column: "KullaniciID");

            migrationBuilder.CreateIndex(
                name: "IX_Siparisler_SiparisDurumuID",
                table: "Siparisler",
                column: "SiparisDurumuID");

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_KategoriID",
                table: "Urunler",
                column: "KategoriID");

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_MarkaID",
                table: "Urunler",
                column: "MarkaID");

            migrationBuilder.CreateIndex(
                name: "IX_YorumAnalizleri_YorumID",
                table: "YorumAnalizleri",
                column: "YorumID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Yorumlar_KullaniciID",
                table: "Yorumlar",
                column: "KullaniciID");

            migrationBuilder.CreateIndex(
                name: "IX_Yorumlar_UrunID",
                table: "Yorumlar",
                column: "UrunID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favoriler");

            migrationBuilder.DropTable(
                name: "Kampanyalar");

            migrationBuilder.DropTable(
                name: "LLSonuclari");

            migrationBuilder.DropTable(
                name: "Odemeler");

            migrationBuilder.DropTable(
                name: "Profiller");

            migrationBuilder.DropTable(
                name: "Resimler");

            migrationBuilder.DropTable(
                name: "SepetItems");

            migrationBuilder.DropTable(
                name: "SiparisDetaylari");

            migrationBuilder.DropTable(
                name: "Sliders");

            migrationBuilder.DropTable(
                name: "YorumAnalizleri");

            migrationBuilder.DropTable(
                name: "Sepetler");

            migrationBuilder.DropTable(
                name: "Siparisler");

            migrationBuilder.DropTable(
                name: "Yorumlar");

            migrationBuilder.DropTable(
                name: "SiparisDurumlari");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Urunler");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropTable(
                name: "Markalar");
        }
    }
}
