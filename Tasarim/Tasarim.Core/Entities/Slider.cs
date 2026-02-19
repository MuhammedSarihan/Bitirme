using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Slider : IEntity
    {
        [Display(Name = "Slider ID")]
        public int ID { get; set; }

        [Display(Name = "Başlık")]
        public string Baslik { get; set; }

        [Display(Name = "Slider Görseli")]
        public string SliderResim { get; set; }

        [Display(Name = "Açıklama")]
        public string SliderAciklama { get; set; }

        [Display(Name = "Link")]
        public string Link { get; set; } // Görsele tıklayınca gideceği yer

        [Display(Name = "Sıra No")]
        public int SiraNo { get; set; }
    }
}