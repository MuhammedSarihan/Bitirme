using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasarim.Core.Entities
{
    public class SiparisDetay : IEntity
    {
        public int ID { get; set; }

        public int SiparisID { get; set; }
        public Siparis Siparis { get; set; }

        public int UrunID { get; set; }
        public Urun Urun { get; set; }
        public int UrunVaryasyonID { get; set; }
        public UrunVaryasyon UrunVaryasyon { get; set; }

        [Display(Name = "Adet")]
        public int Adet { get; set; }

        //  Ürünün fiyatı ileride değişebilir (zam gelebilir).
        // Sipariş verildiği andaki fiyatı buraya kaydetmeliyiz ki raporlarda hata olmasın.
        [Display(Name = "Birim Fiyat")]
        public decimal BirimFiyat { get; set; }
    }
}