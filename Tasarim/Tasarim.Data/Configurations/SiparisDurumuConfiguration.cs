using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tasarim.Core.Entities;

namespace Tasarim.Data.Configurations
{
    public class SiparisDurumuConfiguration : IEntityTypeConfiguration<SiparisDurumu>
    {
        public void Configure(EntityTypeBuilder<SiparisDurumu> builder)
        {
            builder.Property(x => x.DurumAd).IsRequired().HasColumnType("varchar(30)").HasMaxLength(30);

            builder.HasData(
                new SiparisDurumu { ID = 1, DurumAd = "Onay Bekliyor" },
                new SiparisDurumu { ID = 2, DurumAd = "Hazırlanıyor" },
                new SiparisDurumu { ID = 3, DurumAd = "Kargolandı" },
                new SiparisDurumu { ID = 4, DurumAd = "Teslim Edildi" }
            );
        }
    }
}
