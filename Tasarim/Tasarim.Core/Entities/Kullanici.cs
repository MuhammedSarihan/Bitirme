using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Kullanici : IEntity
    {
        [Display(Name = "Kullanıcı ID")]
        public int ID { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAd { get; set; }

        [Display(Name = "Şifre")] // Şifreler genellikle hashlenmiş olarak saklanır ama ilerde yapcaz arkadaşlar
        public string Sifre { get; set; } 

        [Display(Name = "Yönetici mi?")]
        public bool AdminMi { get; set; }

        public bool AktifMi { get; set; } = true; // Varsayılan olarak herkes aktif doğar

        // Navigation Properties
        public Profil? Profil { get; set; }
        public Sepet? Sepet { get; set; }
        public ICollection<Siparis>? Siparisler { get; set; }
        public ICollection<Favori>? Favoriler { get; set; }
        public ICollection<Yorum>? Yorumlar { get; set; }
    }
}