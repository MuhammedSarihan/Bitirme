using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Profil : IEntity
    {
        [Display(Name = "Profil ID")]
        public int ID { get; set; }

        [Display(Name = "Ad")]
        public string Ad { get; set; }

        [Display(Name = "Soyad")]
        public string Soyad { get; set; }

        [Display(Name = "E-Posta")]
        public string? Mail { get; set; }

        [Display(Name = "Telefon")]
        public string? TelNo { get; set; }

        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        public string? Cinsiyet { get; set; }
        public int? Yas { get; set; }
        public double? Boy { get; set; }
        public double? Kilo { get; set; }

        public int KullaniciID { get; set; }
        public Kullanici Kullanici { get; set; }
    }
}