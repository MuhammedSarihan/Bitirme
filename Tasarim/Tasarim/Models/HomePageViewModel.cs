using Tasarim.Core.Entities;

namespace Tasarim.Models
{
    public class HomePageViewModel
    {
        public List<Urun>? Urunler { get; set; }
        public List<Slider>? sliderListesi { get; set; }
        public List<Kampanya>? Kampanyalar { get; set; }

    }
}
