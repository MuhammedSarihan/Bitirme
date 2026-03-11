using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Tasarim.Data;
using Tasarim.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Tasarim.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _context;

        public HomeController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {

            // 1. Ürünleri çekerken kampanya ilişkilerini de dahil ediyoruz (Include)
            var urunler = await _context.Urunler
                .Where(p => p.AktifMi)
                .Include(u => u.KampanyaUrunleri)
                    .ThenInclude(ku => ku.Kampanya)
                .ToListAsync();

            // 2. Ana sayfa için indirimli fiyatları hesaplayıp ViewBag'e atıyoruz
            var indirimliFiyatlar = new Dictionary<int, decimal>();
            foreach (var urun in urunler)
            {
                var aktifKampanyalar = urun.KampanyaUrunleri
                    .Where(ku => ku.Kampanya != null && ku.Kampanya.KampanyaAktifMi) // Sizin tablonuzdaki aktiflik sütunu
                    .Select(ku => ku.Kampanya);

                if (aktifKampanyalar.Any())
                {
                    decimal enDusukFiyat = aktifKampanyalar.Min(k => (int)k.IndirimTipi == 1
                        ? urun.Fiyat - (urun.Fiyat * k.IndirimTutari / 100m)
                        : urun.Fiyat - k.IndirimTutari);

                    indirimliFiyatlar.Add(urun.ID, enDusukFiyat);
                }
                else
                {
                    indirimliFiyatlar.Add(urun.ID, urun.Fiyat);
                }
            }
            ViewBag.IndirimliFiyatlar = indirimliFiyatlar;
            // Sliderları SıraNo'ya göre küçükten büyüğe sıralayarak çekiyoruz (senkron olarak)

            var model = new HomePageViewModel()
            {
                sliderListesi = await _context.Sliders.OrderBy(s => s.SiraNo).ToListAsync(),
                Urunler = await _context.Urunler.Where(p=> p.AktifMi).ToListAsync(),
                Kampanyalar = await _context.Kampanyalar.Where(k => k.KampanyaAktifMi).ToListAsync(),
                Kategoriler = await _context.Kategoriler.Where(k => k.AktifMi).ToListAsync()
            };

            return View(model);
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
