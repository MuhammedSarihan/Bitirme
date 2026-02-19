using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KategorilerController : Controller
    {
        private readonly DatabaseContext _context;

        public KategorilerController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Kategoriler
        // EKLENDİ: Include ile üst kategori adlarını liste ekranına çektik.
        public async Task<IActionResult> Index()
        {
            var kategoriler = await _context.Kategoriler.Include(k => k.UstKategori).ToListAsync();
            return View(kategoriler);
        }

        // GET: Admin/Kategoriler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var kategori = await _context.Kategoriler
                .Include(k => k.UstKategori)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (kategori == null) return NotFound();

            return View(kategori);
        }

        // GET: Admin/Kategoriler/Create
        public IActionResult Create()
        {
            // Dropdown listesini dolduruyoruz
            ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "ID", "KategoriAd");
            return View();
        }

        // POST: Admin/Kategoriler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Kategori kategori)
        {
            // Formdan gelmeyen ilişkisel objeleri doğrulama işleminden çıkarıyoruz.
            ModelState.Remove("UstKategori");
            ModelState.Remove("AltKategoriler"); // Eğer Kategori sınıfında bu isimde bir liste varsa yaz
            ModelState.Remove("Urunler");        // Eğer Kategori sınıfında Urunler listesi varsa yaz
            // ModelState bazen navigasyon property'leri (UstKategori gibi) yüzünden hata verir.
            // Bu yüzden sadece ihtiyacımız olan alanları kontrol etmek en sağlıklısıdır.
            if (ModelState.IsValid)
            {
                _context.Add(kategori);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa listeyi tekrar doldurmalıyız ki sayfa hata verip kapanmasın.
            ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "ID", "KategoriAd");
            return View(kategori);
        }

        // GET: Admin/Kategoriler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var kategori = await _context.Kategoriler.FindAsync(id);
            if (kategori == null) return NotFound();

            // Seçili kategoriyi listeden çıkartıyoruz (Kendisini kendisine üst kategori yapamasın diye)
            var kategoriler = _context.Kategoriler.Where(x => x.ID != id).ToList();
            ViewBag.Kategoriler = new SelectList(kategoriler, "ID", "KategoriAd", kategori.UstKategoriID);

            return View(kategori);
        }

        // POST: Admin/Kategoriler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Kategori kategori)
        {
            if (id != kategori.ID) return NotFound();
            // Aynı şekilde formdan gelmeyen objeleri temizliyoruz.
            ModelState.Remove("UstKategori");
            ModelState.Remove("AltKategoriler");
            ModelState.Remove("Urunler");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kategori);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategoriExists(kategori.ID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "ID", "KategoriAd", kategori.UstKategoriID);
            return View(kategori);
        }

        // GET: Admin/Kategoriler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var kategori = await _context.Kategoriler
                .Include(k => k.UstKategori)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (kategori == null) return NotFound();

            return View(kategori);
        }

        // POST: Admin/Kategoriler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kategori = await _context.Kategoriler.FindAsync(id);
            if (kategori != null)
            {
                _context.Kategoriler.Remove(kategori);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KategoriExists(int id)
        {
            return _context.Kategoriler.Any(e => e.ID == id);
        }
    }
}
