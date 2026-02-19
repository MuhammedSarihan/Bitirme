using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class MarkaConfiguration : IEntityTypeConfiguration<Marka>
    {
        public void Configure(EntityTypeBuilder<Marka> builder)
        {
            builder.Property(x => x.MarkaAd).IsRequired().HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.Logo).HasColumnType("varchar(150)").HasMaxLength(150);

            // --- Örnek Veriler ---
            builder.HasData(
                new Marka { ID = 1, MarkaAd = "Beyoğlu Abiye", AktifMi = true },
                new Marka { ID = 2, MarkaAd = "Tuay", AktifMi = true },
                new Marka { ID = 3, MarkaAd = "Hennin", AktifMi = true }
            );
        }
    }
}