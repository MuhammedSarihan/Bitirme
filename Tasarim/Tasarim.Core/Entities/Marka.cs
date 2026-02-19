using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Marka : IEntity
    {
        [Display(Name = "Marka ID ")]
        public int ID { get; set; }
        [Display(Name = "Marka Ad ")]
        [Required(ErrorMessage = "{0} alanı gereklidir.")]
        public string MarkaAd { get; set; }
        [Display(Name = "Marka Logosu ")]
        public string? Logo { get; set; }

        // İlişkiler
        public ICollection<Urun>? Urunler { get; set; }
        public bool AktifMi { get; set; }
    }
}
