using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class SliderConfiguration : IEntityTypeConfiguration<Slider>
    {
        public void Configure(EntityTypeBuilder<Slider> builder)
        {
            builder.Property(x => x.Baslik).HasColumnType("varchar(100)").HasMaxLength(100);
            builder.Property(x => x.SliderAciklama).HasColumnType("varchar(250)").HasMaxLength(250);
            builder.Property(x => x.SliderResim).HasColumnType("varchar(150)").HasMaxLength(150);
            builder.Property(x => x.Link).HasColumnType("varchar(200)").HasMaxLength(200);
        }
    }
}
