
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using odevVb.Utils; // Dosya yükleme yardımcımız
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class ResimlerController : Controller
    {
        private readonly DatabaseContext _context;

        public ResimlerController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Resimler
        public async Task<IActionResult> Index()
        {
            // Ürünleri başlığa göre sıralı getirelim ki galeri derli toplu dursun
            var databaseContext = _context.Resimler.Include(r => r.Urun).OrderBy(r => r.Urun.Baslik).ThenBy(r => r.SiraNo);
            return View(await databaseContext.ToListAsync());
        }

        // GET: Admin/Resimler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var resim = await _context.Resimler
                .Include(r => r.Urun)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (resim == null) return NotFound();

            return View(resim);
        }

        // GET: Admin/Resimler/Create
        public IActionResult Create()
        {
            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "Baslik");
            return View();
        }

        // POST: Admin/Resimler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resim resim, IFormFile? ResimDosyasi)
        {
            // İlişkisel tablo hatası vermesin diye bunu zaten silmiştik
            ModelState.Remove("Urun");

            // Sistem "ResimYolu boş geldi" diye ağlamasın diye o hatayı görmezden gel diyoruz.
            // Çünkü birazdan aşağıda dosya varsa biz dolduracağız.
            ModelState.Remove("ResimYolu");
            if (ModelState.IsValid)
            {
                // Eğer kullanıcı gerçekten bir dosya seçmişse
                if (ResimDosyasi != null)
                {
                    // Dosyayı yükle ve oluşan yolu ResimYolu özelliğine ata
                    resim.ResimYolu = await FileHelper.FileLoaderAsync(ResimDosyasi);

                    // Artık ResimYolu dolu olduğu için veritabanı kızmayacak.
                    _context.Add(resim);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Eğer dosya SEÇMEMİŞSE, o zaman hata verelim.
                    ModelState.AddModelError("ResimDosyasi", "Lütfen bir fotoğraf dosyası seçiniz.");
                }
            }

            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "Baslik", resim.UrunID);
            return View(resim);
        }

        // GET: Admin/Resimler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var resim = await _context.Resimler.FindAsync(id);
            if (resim == null) return NotFound();

            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "Baslik", resim.UrunID);
            return View(resim);
        }

        // POST: Admin/Resimler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resim resim, IFormFile? ResimDosyasi)
        {
            if (id != resim.ID) return NotFound();

            ModelState.Remove("Urun");
            ModelState.Remove("ResimYolu");
            if (ModelState.IsValid)
            {
                try
                {
                    // Yeni resim seçilmişse eskiyi ezip yenisini yükle
                    if (ResimDosyasi != null)
                    {
                        resim.ResimYolu = await FileHelper.FileLoaderAsync(ResimDosyasi);
                    }

                    _context.Update(resim);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResimExists(resim.ID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "Baslik", resim.UrunID);
            return View(resim);
        }

        // GET: Admin/Resimler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var resim = await _context.Resimler
                .Include(r => r.Urun)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (resim == null) return NotFound();

            return View(resim);
        }

        // POST: Admin/Resimler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resim = await _context.Resimler.FindAsync(id);
            if (resim != null)
            {
                // Sunucudaki fiziksel resmi de çöpe atıyoruz!
                if (!string.IsNullOrEmpty(resim.ResimYolu))
                {
                    FileHelper.FileRemover(resim.ResimYolu);
                }

                _context.Resimler.Remove(resim);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ResimExists(int id)
        {
            return _context.Resimler.Any(e => e.ID == id);
        }
    }
}
