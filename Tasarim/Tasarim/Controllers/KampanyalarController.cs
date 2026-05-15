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

    .Include(k => k.KampanyaUrunleri)
        .ThenInclude(ku => ku.Urun)
            .ThenInclude(u => u.KampanyaUrunleri)
                .ThenInclude(uku => uku.Kampanya)

    .Include(k => k.KampanyaUrunleri)
        .ThenInclude(ku => ku.Urun)
            .ThenInclude(u => u.Varyasyonlar)

    .Include(k => k.KampanyaUrunleri)
        .ThenInclude(ku => ku.Urun)
            .ThenInclude(u => u.Yorumlar)

    .FirstOrDefaultAsync(m => m.ID == id);

            if (kampanya == null)
            {
                return NotFound(); // Kampanya yoksa ürün aramaya gerek kalmadan işlemi bitir
            }
            if (kampanya.KampanyaUrunleri != null)
            {
                foreach (var kampanyaUrunu in kampanya.KampanyaUrunleri)
                {
                    var urun = kampanyaUrunu.Urun;
                    if (urun == null) continue;

                    var aktifKampanyalar = urun.KampanyaUrunleri
                        .Where(ku => ku.Kampanya != null && ku.Kampanya.KampanyaAktifMi)
                        .Select(ku => ku.Kampanya);
                }
            }
            // Ürün listesini View'a model olarak gönderiyoruz
            return View(kampanya);
        }
    }
}
