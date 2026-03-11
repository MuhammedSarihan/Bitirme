using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasarim.Areas.Admin.Models;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{

    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class KampanyaUrunleriController : Controller
    {
        private readonly DatabaseContext _context;

        public KampanyaUrunleriController(DatabaseContext context)
        {
            _context = context;
        }

        // 1. ADIM: Önce hangi kampanyaya ürün ekleyeceğimizi seçmek için listeleme
        public async Task<IActionResult> Index()
        {
            var kampanyalar = await _context.Kampanyalar.ToListAsync();
            return View(kampanyalar);
        }

        // 2. ADIM (GET): Kampanyaya ait ürünleri checkbox listesi olarak getirme
        public async Task<IActionResult> UrunAta(int id)
        {
            // Kampanyayı ve içindeki mevcut ürünleri bul
            var kampanya = await _context.Kampanyalar
                .Include(k => k.KampanyaUrunleri)
                .FirstOrDefaultAsync(k => k.ID == id);

            if (kampanya == null) return NotFound();

            // Sistemdeki TÜM ürünleri getir
            var butunUrunler = await _context.Urunler.ToListAsync();

            // Ekrana göndereceğimiz modeli dolduruyoruz
            var model = new List<KampanyaUrunViewModel>();

            foreach (var urun in butunUrunler)
            {
                model.Add(new KampanyaUrunViewModel
                {
                    UrunID = urun.ID,
                    UrunKod = urun.UrunKod,
                    // Eğer ürün zaten bu kampanyanın içindeyse, SeciliMi = true olacak
                    SeciliMi = kampanya.KampanyaUrunleri.Any(ku => ku.UrunID == urun.ID)
                });
            }

            ViewBag.KampanyaAd = kampanya.KampanyaAd;
            ViewBag.KampanyaID = kampanya.ID;

            return View(model);
        }

        // 3. ADIM (POST): Formdan gelen seçili ürünleri veritabanına kaydetme
        [HttpPost]
        public async Task<IActionResult> UrunAta(int kampanyaId, List<int> secilenUrunler)
        {
            // 1. Önce bu kampanyanın eski ürün kayıtlarını köprü tablosundan tamamen temizle
            var silinecekEskiUrunler = _context.KampanyaUrunleri.Where(ku => ku.KampanyaID == kampanyaId);
            _context.KampanyaUrunleri.RemoveRange(silinecekEskiUrunler);

            // 2. Formdan seçilip gelen (Checkbox'ı işaretli) yeni ürünleri köprü tablosuna ekle
            if (secilenUrunler != null && secilenUrunler.Any())
            {
                foreach (var urunId in secilenUrunler)
                {
                    _context.KampanyaUrunleri.Add(new KampanyaUrunleri
                    {
                        KampanyaID = kampanyaId,
                        UrunID = urunId
                    });
                }
            }

            // Değişiklikleri veritabanına yansıt
            await _context.SaveChangesAsync();

            // İşlem bitince listeye geri dön
            return RedirectToAction("Index");
        }
    }
}
