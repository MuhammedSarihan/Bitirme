using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class KullaniciConfiguration : IEntityTypeConfiguration<Kullanici>
    {
        public void Configure(EntityTypeBuilder<Kullanici> builder)
        {
            builder.Property(x => x.KullaniciAd).IsRequired().HasColumnType("varchar(30)").HasMaxLength(30);
            builder.Property(x => x.Sifre).IsRequired().HasColumnType("varchar(64)").HasMaxLength(64); // Hashlenmiş şifre için

            builder.HasData(
                new Kullanici { ID = 1, KullaniciAd = "Admin", Sifre = "1", AdminMi=true },
                new Kullanici { ID = 2, KullaniciAd = "Kullanıcı", Sifre = "1", AdminMi = false }
            );
        }
    }
}
