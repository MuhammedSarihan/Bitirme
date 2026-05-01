using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class İletisimController : Controller
    {
        private readonly DatabaseContext _context;

        public İletisimController(DatabaseContext context)
        {
            _context = context;
        }

        // 1. GÖREV: Admin tarafında mesajları listeleme
        public async Task<IActionResult> Index()
        {
            // Tüm mesajları en yeni en üstte olacak şekilde çeker
            var mesajlar = await _context.İletisimler.OrderByDescending(x => x.MesajTarihi).ToListAsync();
            return View(mesajlar);
        }

        // 2. GÖREV: Formu doldurup veritabanına kaydetme (Kullanıcı tarafı)
        [HttpPost]
        public IActionResult Index(string AdSoyad, string Email, string Konu, string Mesaj)
        {
            var yeniMesaj = new İletisim
            {
                AdSoyad = AdSoyad,
                Email = Email,
                Konu = Konu,
                Mesaj = Mesaj,
                MesajTarihi = DateTime.Now,
                OkunduMu = false // Varsayılan olarak okunmadı işaretlenir
            };

            _context.İletisimler.Add(yeniMesaj);
            _context.SaveChanges();

            TempData["Success"] = "Mesajınız başarıyla iletildi!";
            return RedirectToAction("ContactUs", "Home");
        }

        // 3. GÖREV: Mesajı "Okundu" olarak işaretleme (Admin tarafı)
        public async Task<IActionResult> OkunduYap(int id)
        {
            var mesaj = await _context.İletisimler.FindAsync(id);
            if (mesaj != null)
            {
                mesaj.OkunduMu = true; // Veritabanındaki IsRead sütununu true yapar
                _context.Update(mesaj);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. GÖREV: Mesajı silme (Admin tarafı)
        public async Task<IActionResult> Sil(int id)
        {
            var mesaj = await _context.İletisimler.FindAsync(id);
            if (mesaj != null)
            {
                _context.İletisimler.Remove(mesaj); // Mesajı veritabanından siler
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
