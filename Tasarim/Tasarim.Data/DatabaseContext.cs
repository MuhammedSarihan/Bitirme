using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;

namespace Tasarim.Data
{
    public class DatabaseContext : DbContext
    {

        // Veritabanı bağlantısı için gerekli yapılandırmayı yapıyoruz (bağlantı adresi appsettings.json'dan alınacak)
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        // --- TABLO TANIMLARI (DbSet) ---
        public DbSet<Urun> Urunler { get; set; }
        public DbSet<Kategori> Kategoriler { get; set; }
        public DbSet<Marka> Markalar { get; set; }
        public DbSet<Resim> Resimler { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Profil> Profiller { get; set; }
        public DbSet<Sepet> Sepetler { get; set; }
        public DbSet<SepetItem> SepetItems { get; set; }
        public DbSet<Favori> Favoriler { get; set; }
        public DbSet<Siparis> Siparisler { get; set; }
        public DbSet<SiparisDetay> SiparisDetaylari { get; set; }
        public DbSet<SiparisDurumu> SiparisDurumlari { get; set; }
        public DbSet<Odeme> Odemeler { get; set; }
        public DbSet<UrunVaryasyon> UrunVaryasyonlari { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<Kampanya> Kampanyalar { get; set; }
        public DbSet<KampanyaUrunleri> KampanyaUrunleri { get; set; }
        public DbSet<İletisim> İletisimler { get; set; }
        public DbSet<Yorum> Yorumlar { get; set; }
        public DbSet<YorumAnaliz> YorumAnalizleri { get; set; }
        public DbSet<LLSonuc> LLSonuclari { get; set; }
        public DbSet<UrunOzellikleri> UrunOzellikleri { get; set; }


        // --- İLİŞKİ VE AYARLAR (Fluent API) ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- MEVCUT İLİŞKİLERİNİZ (Kullanıcı, Profil, Sepet, Analiz, Kampanya) ---
            modelBuilder.Entity<Kullanici>().HasOne(k => k.Profil).WithOne(p => p.Kullanici).HasForeignKey<Profil>(p => p.KullaniciID);
            modelBuilder.Entity<Kullanici>().HasOne(k => k.Sepet).WithOne(s => s.Kullanici).HasForeignKey<Sepet>(s => s.KullaniciID);
            modelBuilder.Entity<Yorum>().HasOne(y => y.YorumAnaliz).WithOne(ya => ya.Yorum).HasForeignKey<YorumAnaliz>(ya => ya.YorumID);
            modelBuilder.Entity<Urun>().HasOne(u => u.LLSonuc).WithOne(l => l.Urun).HasForeignKey<LLSonuc>(l => l.UrunID);
            modelBuilder.Entity<KampanyaUrunleri>().HasKey(cp => new { cp.UrunID, cp.KampanyaID });

            modelBuilder.Entity<KampanyaUrunleri>().HasOne(cp => cp.Urun).WithMany(p => p.KampanyaUrunleri).HasForeignKey(cp => cp.UrunID);
            modelBuilder.Entity<KampanyaUrunleri>().HasOne(cp => cp.Kampanya).WithMany(c => c.KampanyaUrunleri).HasForeignKey(cp => cp.KampanyaID);

            // --- PROFESYONEL EKLEMELER: UNIQUE CONSTRAINTS (BENZERSİZLİK) ---
            // Aynı mail adresiyle ikinci bir kayıt yapılamaz
            modelBuilder.Entity<Profil>().HasIndex(p => p.Mail).IsUnique();
            // Aynı sipariş numarası iki kez üretilemez
            modelBuilder.Entity<Siparis>().HasIndex(s => s.SiparisNo).IsUnique();

            // --- PARA BİRİMLERİ HASSASİYETİ ---
            modelBuilder.Entity<Urun>().Property(u => u.Fiyat).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Siparis>().Property(s => s.ToplamTutar).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SiparisDetay>().Property(sd => sd.BirimFiyat).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Odeme>().Property(o => o.Tutar).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Kampanya>().Property(k => k.IndirimTutari).HasColumnType("decimal(18,2)");
            // --- SİLME DAVRANIŞLARI (DATA INTEGRITY) ---
            // Sipariş silinirse detayları silinsin (Mevcut doğru ayarınız)
            modelBuilder.Entity<Siparis>().HasMany(s => s.SiparisDetaylari).WithOne(sd => sd.Siparis)
                .HasForeignKey(sd => sd.SiparisID).OnDelete(DeleteBehavior.Cascade);

            // Ürün veya Varyasyon silinirse sipariş detayı SİLİNMESİN (Hata versin/Kısıtlasın)
            // Bu sayede geçmiş satış verilerini korumuş oluruz.
            modelBuilder.Entity<SiparisDetay>().HasOne(sd => sd.Urun).WithMany(u => u.SiparisDetaylari)
                .HasForeignKey(sd => sd.UrunID).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SiparisDetay>().HasOne(sd => sd.UrunVaryasyon).WithMany(uv => uv.SiparisDetaylari)
                .HasForeignKey(sd => sd.UrunVaryasyonID).OnDelete(DeleteBehavior.NoAction);

            // --- CONCURRENCY TOKEN (STOK ÇAKIŞMA ÖNLEYİCİ) ---
            // UrunVaryasyon tablosundaki RowVersion alanını DB tarafında mühürlüyoruz.
            modelBuilder.Entity<UrunVaryasyon>().Property(uv => uv.RowVersion).IsRowVersion();

            // Diğer konfigürasyonları uygula
            modelBuilder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);


            // Ürün tablosu ile UrunOzellikleri tablosu arasında 1-1 ilişki tanımlaması.
            // Bu sayede her ürünün yalnızca bir tane AI analiz sonucu olabilir ve UrunID üzerinden birbirine bağlanır.
            

            modelBuilder.Entity<Urun>()
                .HasOne(u => u.UrunOzellikleri)
                .WithOne(uo => uo.Urun)
                .HasForeignKey<UrunOzellikleri>(uo => uo.UrunID);

        }


    }
}
