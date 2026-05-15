using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.ExtensionMethods;

namespace Tasarim.Controllers
{
    public class FavorilerController : Controller
    {
        private readonly DatabaseContext _context;

        public FavorilerController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Urun> favoriUrunler = new List<Urun>();

            // 1. KULLANICI GİRİŞ YAPMIŞSA (Veritabanından Çek)
            if (User.Identity!.IsAuthenticated)
            {
                int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var favorilerDb = await _context.Favoriler
             .Where(f => f.KullaniciID == kullaniciId)
             .Include(f => f.Urun)
                 .ThenInclude(u => u.KampanyaUrunleri)
                     .ThenInclude(ku => ku.Kampanya)
             .Include(f => f.Urun)
                 .ThenInclude(u => u.Varyasyonlar) // Stok (Tükendi) hesabı için
             .Include(f => f.Urun)
                 .ThenInclude(u => u.Yorumlar)     // Yıldız değerlendirmeleri için
             .ToListAsync();

                // View'a sadece Urun listesi göndermek için dönüştürüyoruz
                favoriUrunler = favorilerDb.Select(f => f.Urun).ToList();
            }
            // 2. KULLANICI ZİYARETÇİYSE (Session'dan Çek)
            else
            {
                favoriUrunler = GetFavorilerSession();
            }

            return View(favoriUrunler);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int UrunID)
        {
            var urun = await _context.Urunler.FindAsync(UrunID);
            if (urun == null) return RedirectToAction("Index");

            // 1. KULLANICI GİRİŞ YAPMIŞSA (Veritabanına Kaydet)
            if (User.Identity!.IsAuthenticated)
            {
                int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // Daha önce eklenmiş mi kontrol et
                bool zatenVar = await _context.Favoriler.AnyAsync(f => f.UrunID == UrunID && f.KullaniciID == kullaniciId);

                if (!zatenVar)
                {
                    var yeniFavori = new Favori { KullaniciID = kullaniciId, UrunID = UrunID };
                    await _context.Favoriler.AddAsync(yeniFavori);
                    await _context.SaveChangesAsync();
                }
            }
            // 2. KULLANICI ZİYARETÇİYSE (Session'a Kaydet)
            else
            {
                var favorilerSession = GetFavorilerSession();
                if (!favorilerSession.Any(p => p.ID == UrunID))
                {
                    favorilerSession.Add(urun);
                    HttpContext.Session.SetJson("GetFavoriler", favorilerSession);
                }
            }

            // MÜŞTERİ DENEYİMİ (UX) DOKUNUŞU: 
            // Müşteri "Favoriye Ekle"ye bastığında bulunduğu ürün sayfasında kalsın, zorla Favoriler sayfasına atılmasın.
            string referer = Request.Headers["Referer"].ToString();
            return Redirect(!string.IsNullOrEmpty(referer) ? referer : "/");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int UrunID)
        {
            // 1. KULLANICI GİRİŞ YAPMIŞSA (Veritabanından Sil)
            if (User.Identity!.IsAuthenticated)
            {
                int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var silinecekFavori = await _context.Favoriler
                    .FirstOrDefaultAsync(f => f.UrunID == UrunID && f.KullaniciID == kullaniciId);

                if (silinecekFavori != null)
                {
                    _context.Favoriler.Remove(silinecekFavori);
                    await _context.SaveChangesAsync();
                }
            }
            // 2. KULLANICI ZİYARETÇİYSE (Session'dan Sil)
            else
            {
                var favorilerSession = GetFavorilerSession();
                if (favorilerSession.Any(p => p.ID == UrunID))
                {
                    favorilerSession.RemoveAll(i => i.ID == UrunID);
                    HttpContext.Session.SetJson("GetFavoriler", favorilerSession);
                }
            }

            // MÜŞTERİ DENEYİMİ (UX) DOKUNUŞU: 
            // Müşteri "Favoriye Ekle"ye bastığında bulunduğu ürün sayfasında kalsın, zorla Favoriler sayfasına atılmasın.
            string referer = Request.Headers["Referer"].ToString();
            return Redirect(!string.IsNullOrEmpty(referer) ? referer : "/");
        }

        // --- YARDIMCI METOT (Sadece Session için) ---
        private List<Urun> GetFavorilerSession()
        {
            return HttpContext.Session.GetJson<List<Urun>>("GetFavoriler") ?? new List<Urun>();
        }
    }
}