using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Kullanici : IEntity
    {
        [Display(Name = "Kullanıcı ID")]
        public int ID { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAd { get; set; }

        [Display(Name = "Şifre")]
        public string Sifre { get; set; } // Identity kullanınca burası değişecek ama şemana sadık kaldım.

        [Display(Name = "Yönetici mi?")]
        public bool AdminMi { get; set; }

        // Navigation Properties
        public Profil? Profil { get; set; }
        public Sepet? Sepet { get; set; }
        public ICollection<Siparis>? Siparisler { get; set; }
        public ICollection<Favori>? Favoriler { get; set; }
        public ICollection<Yorum>? Yorumlar { get; set; }
    }
}