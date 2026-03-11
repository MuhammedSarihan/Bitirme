using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.ExtensionMethods;
using System.Linq;

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
            var databaseContext = _context.Urunler.Where(p=>p.AktifMi && p.Baslik.Contains(q)|| p.Aciklama.Contains(q) || p.ModelKodu.Contains(q))
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .Include(u => u.KampanyaUrunleri)
                    .ThenInclude(ku => ku.Kampanya);
            var urunlerListesi = await databaseContext.ToListAsync();

            var indirimliFiyatlar = new Dictionary<int, decimal>();
            foreach (var urun in urunlerListesi)
            {
                var aktifKampanyalar = urun.KampanyaUrunleri
                    .Where(ku => ku.Kampanya != null && ku.Kampanya.KampanyaAktifMi)
                    .Select(ku => ku.Kampanya);

                if (aktifKampanyalar.Any())
                {
                    // İndirim Tipi 1: Yüzde, 2: Sabit (Veritabanınızdaki int karşılığına göre güncelleyebilirsiniz)
                    decimal enDusukFiyat = aktifKampanyalar.Min(k => (int)k.IndirimTipi == 1
                        ? urun.Fiyat - (urun.Fiyat * k.IndirimTutari / 100m)
                        : urun.Fiyat - k.IndirimTutari);

                    indirimliFiyatlar.Add(urun.ID, enDusukFiyat);
                }
                else
                {
                    // Kampanya yoksa orijinal fiyatı ekle
                    indirimliFiyatlar.Add(urun.ID, urun.Fiyat);
                }
            }
            ViewBag.IndirimliFiyatlar = indirimliFiyatlar;

            return View(urunlerListesi);
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
                .Include(u => u.KampanyaUrunleri)
                    .ThenInclude(ku => ku.Kampanya)  
                .FirstOrDefaultAsync(u => u.ID == id);

            if (urun == null || !urun.AktifMi)
            {
                // Ürün yoksa veya admin panelinden "Pasif" yapılmışsa müşteri göremez
                return RedirectToAction("Index", "Home");
            }

            decimal gecerliFiyat = urun.Fiyat;
            var aktifKampanyalar = urun.KampanyaUrunleri
                .Where(ku => ku.Kampanya != null && ku.Kampanya.KampanyaAktifMi)
                .Select(ku => ku.Kampanya);

            if (aktifKampanyalar.Any())
            {
                gecerliFiyat = aktifKampanyalar.Min(k => (int)k.IndirimTipi == 1
                    ? urun.Fiyat - (urun.Fiyat * k.IndirimTutari / 100m)
                    : urun.Fiyat - k.IndirimTutari);
            }
            // Bu bilgiyi View'da fiyat alanında kullanmak için gönderiyoruz
            ViewBag.IndirimliFiyat = gecerliFiyat;


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
            // --- ÜRÜN FAVORİLERDE EKLİ Mİ KONTROLÜ ---
            bool favorideMi = false;

            if (User.Identity!.IsAuthenticated)
            {
                // Giriş yapmışsa veritabanından bak
                int kullaniciId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
                favorideMi = _context.Favoriler.Any(f => f.UrunID == id && f.KullaniciID == kullaniciId);
            }
            else
            {
                // Ziyaretçiyse Session'dan bak
                var favorilerSession = HttpContext.Session.GetJson<List<Urun>>("GetFavoriler") ?? new List<Urun>();
                favorideMi = favorilerSession.Any(u => u.ID == id);
            }

            // Bu bilgiyi butonu değiştirmek için View'a gönderiyoruz
            ViewBag.FavorideMi = favorideMi;
            return View(urun);
        }
    }
}
