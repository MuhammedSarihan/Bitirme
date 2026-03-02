using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasarim.Data;

namespace Tasarim.Controllers
{
    public class UrunlerController : Controller
    {
        private readonly DatabaseContext _context;

        public UrunlerController(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string q="")
        {
            var databaseContext = _context.Urunler.Where(p=>p.AktifMi && p.Baslik.Contains(q)|| p.Aciklama.Contains(q) || p.ModelKodu.Contains(q)).Include(u => u.Kategori).Include(u => u.Marka);
            return View(await databaseContext.ToListAsync());
        }
        // URL: /Urunler/Details/1
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home"); // ID yoksa ana sayfaya at
            }

            // 1. Ürünü ve içindeki Resim, Varyasyon (Beden), Kategori ve Marka listelerini çekiyoruz
            var urun = await _context.Urunler
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .Include(u => u.Resimler)
                .Include(u => u.Varyasyonlar)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (urun == null || !urun.AktifMi)
            {
                // Ürün yoksa veya admin panelinden "Pasif" yapılmışsa müşteri göremez
                return RedirectToAction("Index", "Home");
            }

            // 2. AYNI GRUP (FARKLI RENK) ÜRÜNLERİ BULMA
            // Eğer ürünün bir "Model Kodu" varsa (Örn: BEY3122), o koda sahip DİĞER ürünleri bul
            if (!string.IsNullOrEmpty(urun.ModelKodu))
            {
                var digerRenkler = await _context.Urunler
                    .Where(u => u.ModelKodu == urun.ModelKodu && u.ID != urun.ID && u.AktifMi)
                    .ToListAsync();

                // Sayfada göstermek için ViewBag ile gönderiyoruz
                ViewBag.AyniGrupUrunler = digerRenkler;
            }

            return View(urun);
        }
    }
}
