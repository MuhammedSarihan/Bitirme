using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasarim.Service.Abstract;

namespace Tasarim.Controllers 
{
   
    [Authorize]
    public class SepetController : Controller
    {
        private readonly ISepetService _sepetService;

        public SepetController(ISepetService sepetService)
        {
            _sepetService = sepetService;
        }

        // SEPETİM SAYFASI (GET)
        public async Task<IActionResult> Index()
        {
            // Sisteme giriş yapmış kişinin ID'sini güvenli bir şekilde çerezden (Cookie/Claim) alıyoruz
            int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Servisi kullanarak o kişiye ait sepeti veritabanından çekiyoruz
            var sepet = await _sepetService.GetSepetByKullaniciIdAsync(kullaniciId);

            // Sepetin toplam tutarını hesaplatıp sayfaya (ViewBag ile) yolluyoruz
            ViewBag.ToplamTutar = await _sepetService.ToplamTutarAsync(kullaniciId);

            return View(sepet);
        }

        // SEPETE ÜRÜN EKLEME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(int UrunVaryasyonID, int miktar = 1)
        {
            int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Servise "Bu kullanıcıya, şu bedenden, şu kadar adet ekle" komutunu veriyoruz
            await _sepetService.UrunEkleAsync(kullaniciId, UrunVaryasyonID, miktar);

            // Başarıyla eklendikten sonra müşteriyi direkt kendi sepetine yönlendiriyoruz
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guncelle(int UrunVaryasyonID, int miktar)
        {
            int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Servisimizdeki mevcut metodu çağırıyoruz
            await _sepetService.UrunGuncelleAsync(kullaniciId, UrunVaryasyonID, miktar);

            return RedirectToAction(nameof(Index));
        }
        // SEPETTEN ÜRÜN SİLME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int UrunVaryasyonID)
        {
            int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _sepetService.UrunSilAsync(kullaniciId, UrunVaryasyonID);

            return RedirectToAction(nameof(Index));
        }
    }
}