using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class OdemeConfiguration : IEntityTypeConfiguration<Odeme>
    {
        public void Configure(EntityTypeBuilder<Odeme> builder)
        {
            builder.Property(x => x.IslemNo).HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.OdemeDurumu).HasColumnType("varchar(20)").HasMaxLength(20);
            builder.Property(x => x.Tutar).HasColumnType("decimal(18,2)");
        }
    }
}
