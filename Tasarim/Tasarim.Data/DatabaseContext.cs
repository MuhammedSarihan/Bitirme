using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;

namespace Tasarim.Data
{
    public class DatabaseContext : DbContext
    {
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

        public DbSet<Slider> Sliders { get; set; }
        public DbSet<Kampanya> Kampanyalar { get; set; }

        public DbSet<Yorum> Yorumlar { get; set; }
        public DbSet<YorumAnaliz> YorumAnalizleri { get; set; }
        public DbSet<LLSonuc> LLSonuclari { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {// Kendi SQL Server'ınıza göre düzenleyebilirsiniz -> @"Server=PC_ADINIZ;
            //DESKTOP - MUUA55B - (Şevval) - (Rafiga)      
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-MUUA55B; Database=dbTasarim; Trusted_Connection=True; TrustServerCertificate=True;");
            base.OnConfiguring(optionsBuilder);
        }

        // --- İLİŞKİ VE AYARLAR (Fluent API) ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Kullanıcı - Profil (1-1 İlişki)
            modelBuilder.Entity<Kullanici>()
                .HasOne(k => k.Profil)
                .WithOne(p => p.Kullanici)
                .HasForeignKey<Profil>(p => p.KullaniciID);

            // 2. Kullanıcı - Sepet (1-1 İlişki)
            modelBuilder.Entity<Kullanici>()
                .HasOne(k => k.Sepet)
                .WithOne(s => s.Kullanici)
                .HasForeignKey<Sepet>(s => s.KullaniciID);

            // 3. Yorum - YorumAnaliz (1-1 İlişki)
            modelBuilder.Entity<Yorum>()
                .HasOne(y => y.YorumAnaliz)
                .WithOne(ya => ya.Yorum)
                .HasForeignKey<YorumAnaliz>(ya => ya.YorumID);

            // 4. Ürün - LLSonuc (1-1 İlişki)
            modelBuilder.Entity<Urun>()
                .HasOne(u => u.LLSonuc)
                .WithOne(l => l.Urun)
                .HasForeignKey<LLSonuc>(l => l.UrunID);


            // 5. Para Alanları İçin Hassasiyet Ayarı
            // SQL'de "decimal(18,2)" olarak tutulmasını sağlar.
            modelBuilder.Entity<Urun>().Property(u => u.Fiyat).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Siparis>().Property(s => s.ToplamTutar).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SiparisDetay>().Property(sd => sd.BirimFiyat).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Odeme>().Property(o => o.Tutar).HasColumnType("decimal(18,2)");

            // DeleteBehavior Ayarları 
            // Örneğin: Bir Sipariş silinirse, Detayları da silinsin mi? 
            modelBuilder.Entity<Siparis>()
                .HasMany(s => s.SiparisDetaylari)
                .WithOne(sd => sd.Siparis)
                .HasForeignKey(sd => sd.SiparisID)
                .OnDelete(DeleteBehavior.Cascade);
            // Bu projede IEntityTypeConfiguration uygulayan ne varsa bul ve uygula
            modelBuilder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }


    }
}
