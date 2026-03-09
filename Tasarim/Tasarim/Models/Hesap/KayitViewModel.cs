using System.ComponentModel.DataAnnotations;

namespace Tasarim.Models.Hesap
{
    public class KayitViewModel
    {
        // --- HESAP BİLGİLERİ ---
        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "{0} zorunludur.")]
        public string KullaniciAd { get; set; }

        [Display(Name = "Şifre")]
        [Required(ErrorMessage = "{0} zorunludur.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; }

        [Display(Name = "Şifre Tekrar")]
        [Required(ErrorMessage = "{0} zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("Sifre", ErrorMessage = "Şifreler birbiriyle uyuşmuyor.")]
        public string SifreTekrar { get; set; }

        // --- KİŞİSEL BİLGİLER ---
        [Display(Name = "Ad")]
        [Required(ErrorMessage = "{0} zorunludur.")]
        public string Ad { get; set; }

        [Display(Name = "Soyad")]
        [Required(ErrorMessage = "{0} zorunludur.")]
        public string Soyad { get; set; }

        [Display(Name = "Telefon")]
        [Required(ErrorMessage = "{0} zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon giriniz.")]
        public string Tel { get; set; }
        [Display(Name = "Telefon")]
        [Required(ErrorMessage = "{0} zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir mail giriniz.")]
        public string Mail { get; set; }

        [Display(Name = "Adres")]
        [Required(ErrorMessage = "{0} zorunludur.")]
        public string Adres { get; set; }

        [Display(Name = "Cinsiyet")]
        public string Cinsiyet { get; set; }

        public int? Yas { get; set; }
        public double? Boy { get; set; }
        public double? Kilo { get; set; }
    }
}