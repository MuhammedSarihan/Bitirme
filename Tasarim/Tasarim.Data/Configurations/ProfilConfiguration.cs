using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class ProfilConfiguration : IEntityTypeConfiguration<Profil>
    {
        public void Configure(EntityTypeBuilder<Profil> builder)
        {
            builder.Property(x => x.Ad).IsRequired().HasColumnType("varchar(30)").HasMaxLength(30);
            builder.Property(x => x.Soyad).IsRequired().HasColumnType("varchar(30)").HasMaxLength(30);
            builder.Property(x => x.Cinsiyet).HasColumnType("varchar(5)").HasMaxLength(5); // "Erkek", "Kadın"
            builder.Property(x => x.TelNo).HasColumnType("varchar(15)").HasMaxLength(15);
            builder.Property(x => x.Mail).IsRequired().HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.Adres).HasColumnType("varchar(250)").HasMaxLength(250);


            builder.HasData(
               new Profil { ID = 1, Ad = "Admin",Soyad="1",Adres="Üsküdar",Mail="info@admin.com", KullaniciID = 1 },
               new Profil { ID = 2, Ad = "Kullanıcı",Soyad="1",Adres="Ümraniye",Mail="info@kullanıcı.com", KullaniciID = 2 }
           );
        }
    }
}
