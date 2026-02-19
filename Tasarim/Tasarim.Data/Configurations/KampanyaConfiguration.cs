using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class KampanyaConfiguration : IEntityTypeConfiguration<Kampanya>
    {
        public void Configure(EntityTypeBuilder<Kampanya> builder)
        {
            builder.Property(x => x.KampanyaAd).HasColumnType("varchar(100)").HasMaxLength(100);
            builder.Property(x => x.Aciklama).HasColumnType("varchar(250)").HasMaxLength(250);
            builder.Property(x => x.KampanyaResmi).HasColumnType("varchar(150)").HasMaxLength(150);
        }
    }
}
