using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Yorum : IEntity
    {
        public int ID { get; set; }

        [Display(Name = "Puan")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır.")]
        public int Puan { get; set; }

        [Display(Name = "Yorum")]
        public string YorumIcerik { get; set; }

        public int KullaniciID { get; set; }
        public Kullanici Kullanici { get; set; }

        public int UrunID { get; set; }
        public Urun Urun { get; set; }

        public YorumAnaliz YorumAnaliz { get; set; }
    }
}