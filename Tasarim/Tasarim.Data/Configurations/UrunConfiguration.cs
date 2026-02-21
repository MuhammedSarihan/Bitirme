using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class UrunConfiguration : IEntityTypeConfiguration<Urun>
    {
        public void Configure(EntityTypeBuilder<Urun> builder)
        {
            builder.Property(x => x.UrunKod).IsRequired().HasColumnType("varchar(20)").HasMaxLength(20);
            builder.Property(x => x.Baslik).IsRequired().HasColumnType("varchar(100)").HasMaxLength(100);
            builder.Property(x => x.Aciklama).HasColumnType("varchar(1000)").HasMaxLength(1000); // Max yerine 1000 karakter
            builder.Property(x => x.AnaResim).HasColumnType("varchar(150)").HasMaxLength(150);
            // Fiyat Hassasiyeti
            builder.Property(x => x.Fiyat).HasColumnType("decimal(18,2)");

            // --- Örnek Veriler ---
            //builder.HasData(
            //    new Urun { ID = 1, UrunKod = "BEY3122", MarkaID = 1,Baslik ="Deneme Urunu 1",Beden = "38",Renk="Siyah",Aciklama="Deneme",Stok=100,Fiyat=13500,KategoriID=1, AktifMi = true },
            //    new Urun { ID = 2, UrunKod = "TUA3122", MarkaID = 2,Baslik ="Deneme Urunu 2",Beden = "38",Renk="Siyah",Aciklama="Deneme",Stok=100,Fiyat=13000,KategoriID=1, AktifMi = true },
            //    new Urun { ID = 3, UrunKod = "HEN3122", MarkaID = 3,Baslik ="Deneme Urunu 3",Beden = "38",Renk="Siyah",Aciklama="Deneme",Stok=100,Fiyat=14000,KategoriID=1, AktifMi = true }
            //);
        }
    }
}
