using Tasarim.Core.Entities;

namespace Tasarim.Service.Abstract
{
    public interface ISepetService
    {
        // Sepeti ve içindeki ürünleri getir
        Task<Sepet?> GetSepetByKullaniciIdAsync(int kullaniciId);

        // Sepete belirli bir bedenden (varyasyon) ürün ekle
        Task UrunEkleAsync(int kullaniciId, int urunVaryasyonId, int miktar);

        // Sepetteki ürünün miktarını güncelle
        Task UrunGuncelleAsync(int kullaniciId, int urunVaryasyonId, int miktar);

        // Sepetten belirli bir ürünü sil
        Task UrunSilAsync(int kullaniciId, int urunVaryasyonId);

        // Sepet toplam tutarını hesapla
        Task<decimal> ToplamTutarAsync(int kullaniciId);

        // Satın alma sonrası veya istek üzerine sepeti tamamen boşalt
        Task HepsiniSilAsync(int kullaniciId);
    }
}