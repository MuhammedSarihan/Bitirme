using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Tasarim.Core.Entities
{
    public class UrunOzellikleri : IEntity
    {
        [Key]
        public int ID { get; set; }

        public int UrunID { get; set; }

        // Standart Bilgiler
        public string? AnaKategori { get; set; }
        public string? AnaRenk { get; set; }
        public string? Materyal { get; set; }
        public string? Stil { get; set; }

        // Yapay zekanın tüm "ekstra" keşifleri buraya akacak
        public string? Detaylar { get; set; }

        public DateTime AnalizTarihi { get; set; } = DateTime.Now;

        [ForeignKey("UrunID")]
        public virtual Urun Urun { get; set; }
    }
}
