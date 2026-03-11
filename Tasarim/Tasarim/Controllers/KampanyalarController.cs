using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tasarim.Data;

namespace Tasarim.Controllers
{
    public class KampanyalarController : Controller
    {
        private readonly DatabaseContext _context;

        public KampanyalarController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Kullanici/Kampanyalar

        public async Task<IActionResult> Index()
        {
            return View(await _context.Kampanyalar.ToListAsync());
        }

        // GET: Kullanici/Kampanyalar/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            // 1. ID hiç gönderilmediyse hata dön
            if (id == null)
            {
                return NotFound();
            }

            // 2. ÖNCE KAMPANYAYI BUL: Böyle bir kampanya gerçekten var mı?
            var kampanya = await _context.Kampanyalar
                    .Include(k => k.KampanyaUrunleri)     // Önce köprü tablosunu dahil et
                        .ThenInclude(ku => ku.Urun)       // Sonra o köprü üzerinden Ürünlere ulaş
                            .ThenInclude(u => u.KampanyaUrunleri)
                                .ThenInclude(uku => uku.Kampanya)
                    .FirstOrDefaultAsync(m => m.ID == id);

            if (kampanya == null)
            {
                return NotFound(); // Kampanya yoksa ürün aramaya gerek kalmadan işlemi bitir
            }
            var indirimliFiyatlar = new Dictionary<int, decimal>();

            if (kampanya.KampanyaUrunleri != null)
            {
                foreach (var kampanyaUrunu in kampanya.KampanyaUrunleri)
                {
                    var urun = kampanyaUrunu.Urun;
                    if (urun == null) continue;

                    var aktifKampanyalar = urun.KampanyaUrunleri
                        .Where(ku => ku.Kampanya != null && ku.Kampanya.KampanyaAktifMi)
                        .Select(ku => ku.Kampanya);

                    if (aktifKampanyalar.Any())
                    {
                        decimal enDusukFiyat = aktifKampanyalar.Min(k => (int)k.IndirimTipi == 1
                            ? urun.Fiyat - (urun.Fiyat * k.IndirimTutari / 100m)
                            : urun.Fiyat - k.IndirimTutari);

                        // Aynı ürün birden fazla kez gelirse sözlüğün hata vermesini engellemek için kontrol
                        if (!indirimliFiyatlar.ContainsKey(urun.ID))
                        {
                            indirimliFiyatlar.Add(urun.ID, enDusukFiyat);
                        }
                    }
                    else
                    {
                        if (!indirimliFiyatlar.ContainsKey(urun.ID))
                        {
                            indirimliFiyatlar.Add(urun.ID, urun.Fiyat);
                        }
                    }
                }
            }
            ViewBag.IndirimliFiyatlar = indirimliFiyatlar;
            // Ürün listesini View'a model olarak gönderiyoruz
            return View(kampanya);
        }
    }
}
