using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class SiparisConfiguration : IEntityTypeConfiguration<Siparis>
    {
        public void Configure(EntityTypeBuilder<Siparis> builder)
        {
            builder.Property(x => x.SiparisNo).IsRequired().HasColumnType("varchar(20)").HasMaxLength(20);
            builder.Property(x => x.KargoNo).HasColumnType("varchar(20)").HasMaxLength(20);
            builder.Property(x => x.ToplamTutar).HasColumnType("decimal(18,2)");
        }
    }
}
