using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class KategoriConfiguration : IEntityTypeConfiguration<Kategori>
    {
        public void Configure(EntityTypeBuilder<Kategori> builder)
        {
            builder.Property(x => x.KategoriAd).HasColumnType("varchar(100)").HasMaxLength(100);
            builder.HasData(
        new Kategori { ID = 1, KategoriAd = "Erkek", SiraNo = 1, AktifMi = true, UstteGoster = true },
        new Kategori { ID = 2, KategoriAd = "Kadin", SiraNo = 2, AktifMi = true, UstteGoster = true },
        new Kategori { ID = 3, KategoriAd = "Elektronik", SiraNo = 3, AktifMi = true, UstteGoster = false } // Eksik olan 3 numaralı kategori eklendi!
    );
        }
    }
}
