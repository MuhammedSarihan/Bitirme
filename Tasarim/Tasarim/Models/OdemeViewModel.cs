using System.ComponentModel.DataAnnotations;

namespace Tasarim.Models
{
    public class OdemeViewModel
    {
        // Teslimat Bilgileri
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        public string AdSoyad { get; set; }

        [Required(ErrorMessage = "Telefon zorunludur.")]
        public string Telefon { get; set; }

        [Required(ErrorMessage = "Teslimat adresi zorunludur.")]
        public string TeslimatAdresi { get; set; }

        // Kredi Kartı Bilgileri (Veritabanına KAYDEDİLMEYECEK)
        [Required(ErrorMessage = "Kart üzerindeki isim zorunludur.")]
        public string KartSahibi { get; set; }

        [Required(ErrorMessage = "Kart numarası zorunludur.")]
        public string KartNumarasi { get; set; }

        [Required(ErrorMessage = "Ay zorunludur.")]
        public string SonKullanmaAy { get; set; }

        [Required(ErrorMessage = "Yıl zorunludur.")]
        public string SonKullanmaYil { get; set; }

        [Required(ErrorMessage = "CVV zorunludur.")]
        public string Cvv { get; set; }

        // Sepet Toplamı (Ekranda göstermek için)
        public decimal OdenecekTutar { get; set; }
    }
}
