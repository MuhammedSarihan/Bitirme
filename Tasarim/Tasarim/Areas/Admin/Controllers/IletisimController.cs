using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.Areas.Admin.Models;
using Tasarim.Service.Concrate.LLM;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class İletisimController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly YorumAnalizYoneticisi _analizYoneticisi;
        private readonly KumelemeYoneticisi _kumelemeYoneticisi;

        public İletisimController(DatabaseContext context, YorumAnalizYoneticisi analizYoneticisi, KumelemeYoneticisi kumelemeYoneticisi)
        {
            _context = context;
            _analizYoneticisi = analizYoneticisi;
            _kumelemeYoneticisi = kumelemeYoneticisi;
        }

        // Admin tarafında mesajları listeleme
        // Admin tarafında mesajları listeleme
        public async Task<IActionResult> Index()
        {
            var mesajlar = await _context.İletisimler.OrderByDescending(x => x.MesajTarihi).ToListAsync();

            // Yorumları çekerken Profil ve Urun tablolarını da sorguya dahil (Include) ediyoruz
            var yasakliYorumlar = await _context.Yorumlar
                .Include(y => y.Profil) // Profil tablosundaki Ad, Soyad vb. veriler için
                .Include(y => y.Urun)   // Urun tablosundaki UrunKod vb. veriler için
                .Where(y => y.AnalizEdilirMi == 2 || y.AnalizEdilirMi == 3)
                .ToListAsync();

            var model = new YorumKontroluViewModel
            {
                Mesajlar = mesajlar,
                YasakliYorumlar = yasakliYorumlar
            };

            return View(model);
        }

        // Formu doldurup veritabanına kaydetme
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

        // Mesajı "Okundu" olarak işaretleme
        public async Task<IActionResult> OkunduYap(int id)
        {
            var mesaj = await _context.İletisimler.FindAsync(id);
            if (mesaj != null)
            {
                mesaj.OkunduMu = true; // Veritabanındaki OkunduMu sütununu true yapar
                _context.Update(mesaj);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Mesajı silme 
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
        public async Task<IActionResult> YorumKabulEt(int id)
        {
            var yorum = await _context.Yorumlar.FindAsync(id);
            if (yorum != null)
            {
                yorum.YasakliKelime = false; // Onaylandı, değer false yapıldı
                yorum.AnalizEdilirMi = 1; // Analiz edilecek olarak işaretlendi

                _context.Update(yorum);
                await _context.SaveChangesAsync();

                await _analizYoneticisi.BekleyenYorumlariAnalizEtAsync();
                await _kumelemeYoneticisi.UrunleriKumeleVeAnalizEtAsync();
                TempData["Success"] = "Yorum onaylandı ve analiz edildi.";
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> YorumSil(int id)
        {
            var yorum = await _context.Yorumlar.FindAsync(id);
            if (yorum != null)
            {
                _context.Yorumlar.Remove(yorum); // Veritabanından silinir
                await _context.SaveChangesAsync();

                TempData["Success"] = "Yasaklı yorum silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
