using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class ResimConfiguration : IEntityTypeConfiguration<Resim>
    {
        public void Configure(EntityTypeBuilder<Resim> builder)
        {
            builder.Property(x => x.ResimYolu).IsRequired().HasColumnType("varchar(200)").HasMaxLength(200);
        }
    }
}
