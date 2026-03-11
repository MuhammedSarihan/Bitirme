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
                    .FirstOrDefaultAsync(m => m.ID == id);

            if (kampanya == null)
            {
                return NotFound(); // Kampanya yoksa ürün aramaya gerek kalmadan işlemi bitir
            }

            // Ürün listesini View'a model olarak gönderiyoruz
            return View(kampanya);
        }
    }
}
