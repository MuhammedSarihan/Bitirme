using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    // İndirim tiplerini belirlediğimiz Enum
    public enum IndirimTipi
    {
        [Display(Name = "Yüzde (%)")]
        Yuzde = 1,

        [Display(Name = "Sabit Tutar (TL)")]
        SabitTutar = 2
    }
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

        [Display(Name = "İndirim Tipi")]
        public IndirimTipi IndirimTipi { get; set; }

        [Display(Name = "İndirim Tutarı / Oranı")]
        public decimal IndirimTutari { get; set; }

        public ICollection<KampanyaUrunleri>? KampanyaUrunleri { get; set; }

    }
}