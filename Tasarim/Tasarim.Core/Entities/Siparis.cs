using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Siparis : IEntity
    {
        public int ID { get; set; }

        [Display(Name = "Sipariş No")]
        public string SiparisNo { get; set; }

        [Display(Name = "Tarih")]
        public DateTime SiparisTarihi { get; set; }

        [Display(Name = "Tutar")]
        public decimal ToplamTutar { get; set; }

        [Display(Name = "Kargo No")]
        public string? KargoNo { get; set; }

        public int KullaniciID { get; set; }
        public Kullanici Kullanici { get; set; }

        public int SiparisDurumuID { get; set; }
        public SiparisDurumu? SiparisDurumu { get; set; }

        // Yeni: Bir siparişin birden çok kalemi olur.
        public ICollection<SiparisDetay>? SiparisDetaylari { get; set; }

        public ICollection<Odeme>? Odemeler { get; set; }
    }
}