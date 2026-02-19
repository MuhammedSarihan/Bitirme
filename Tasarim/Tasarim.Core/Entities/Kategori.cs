using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasarim.Core.Entities
{
    public class Kategori : IEntity
    {
        [Display(Name = "Kategori ID ")]
        public int ID { get; set; }
        [Display(Name = "Kategori AD ")]
        public string KategoriAd { get; set; }
        [Display(Name = "Kategori Görseli")]
        public string? KategoriResmi { get; set; }
        [Display(Name = "Sıra No")]
        public int SiraNo { get; set; }
        [Display(Name = "Aktif mi ? ")]
        public bool AktifMi { get; set; }

        [Display(Name = "Üst Kategori ID ")]
        public int? UstKategoriID { get; set; }

        [Display(Name = "Üst Menüde Göster ")]
        public bool UstteGoster { get; set; }

        // Eğer sorun varsa, aşağıdaki kodu kullanarak kendine referans veren ilişki kurulabilir.
        //[ForeignKey("UstKategoriID")]
        //public Kategori UstKategori { get; set; }

        // İlişkiler
        public ICollection<Urun>? Urunler { get; set; }
    }
}
