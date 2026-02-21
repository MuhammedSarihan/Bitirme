using Tasarim.Core.Entities;

namespace Tasarim.Areas.Admin.Models
{
    public class TopluVaryasyonViewModel
    {
        // Tüm varyasyonların bağlanacağı tek bir ortak ürün
        public int SecilenUrunID { get; set; }

        // Bu ürüne ait beden ve stok listesi
        public List<UrunVaryasyon> Varyasyonlar { get; set; } = new List<UrunVaryasyon>();
    }
}