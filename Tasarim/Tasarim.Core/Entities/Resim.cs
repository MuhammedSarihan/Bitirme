using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Resim : IEntity
    {
        [Display(Name = "Resim ID")]
        public int ID { get; set; }

        [Display(Name = "Resim Yolu")]
        public string ResimYolu { get; set; }

        [Display(Name = "Sıra No")]
        public int? SiraNo { get; set; }

        public int? UrunID { get; set; }
        public Urun? Urun { get; set; }
    }
}