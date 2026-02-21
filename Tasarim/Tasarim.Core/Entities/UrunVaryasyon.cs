using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class UrunVaryasyon : IEntity
    {
        public int ID { get; set; }

        [Display(Name = "Beden")]
        [Required(ErrorMessage = "{0} bilgisi zorunludur.")]
        public string Beden { get; set; } // Örn: 38, 40, S, M, L...

        [Display(Name = "Stok Adedi")]
        [Required(ErrorMessage = "{0} zorunludur.")]
        public int StokAdedi { get; set; }

        // İlişki (Foreign Key): Bu beden/stok bilgisi hangi ürüne ait?
        public int UrunID { get; set; }
        public Urun Urun { get; set; }
    }
}