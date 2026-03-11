using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Kampanya : IEntity
    {
        [Display(Name = "Kampanya ID")]
        public int ID { get; set; }

        [Display(Name = "Kampanya Adı")]
        public string KampanyaAd { get; set; }

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Kampanya Görseli")]
        public string? KampanyaResmi { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool KampanyaAktifMi { get; set; }

        public ICollection<KampanyaUrunleri>? KampanyaUrunleri { get; set; }

    }
}