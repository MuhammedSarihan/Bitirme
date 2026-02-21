using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UrunVaryasyonlarController : Controller
    {
        private readonly DatabaseContext _context;

        public UrunVaryasyonlarController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/UrunVaryasyonlar
        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.UrunVaryasyonlari.Include(u => u.Urun);
            return View(await databaseContext.ToListAsync());
        }

        // GET: Admin/UrunVaryasyonlar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var urunVaryasyon = await _context.UrunVaryasyonlari
                .Include(u => u.Urun)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (urunVaryasyon == null)
            {
                return NotFound();
            }

            return View(urunVaryasyon);
        }

        // GET: Admin/UrunVaryasyonlar/Create
        public IActionResult Create()
        {
            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "UrunKod");
            // Ekrana başlangıçta 5 adet boş beden/stok satırı gönderiyoruz
            var model = new Models.TopluVaryasyonViewModel();
            model.Varyasyonlar = new List<UrunVaryasyon>();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.TopluVaryasyonViewModel model)
        {
            // Sadece "Beden" kısmı doldurulmuş ve stoğu 0'dan büyük olan satırları ayıklıyoruz
            var eklenecekVaryasyonlar = model.Varyasyonlar
                .Where(v => !string.IsNullOrWhiteSpace(v.Beden) && v.StokAdedi > 0)
                .ToList();

            // Ürün seçilmişse ve en az 1 tane geçerli beden girilmişse işlemleri yap
            if (model.SecilenUrunID > 0 && eklenecekVaryasyonlar.Any())
            {
                foreach (var varyasyon in eklenecekVaryasyonlar)
                {

                    // 1. ADIM: Veritabanında aynı Ürün ve aynı Beden var mı?
                    var varolanVaryasyon = await _context.UrunVaryasyonlari
                        .FirstOrDefaultAsync(v => v.UrunID == varyasyon.UrunID && v.Beden == varyasyon.Beden);

                    if (varolanVaryasyon != null)
                    {
                        // 2. ADIM (VARSA): Sadece mevcut stoğun üzerine ekle
                        varolanVaryasyon.StokAdedi += varyasyon.StokAdedi;
                        _context.Update(varolanVaryasyon);
                    }
                    else
                    {
                        // 3. ADIM (YOKSA): Normal kayıt yap
                        _context.Add(varyasyon);
                    }
                }

                // Tüm liste döngüden çıktıktan sonra tek seferde kaydet
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata veya eksik giriş varsa listeyi tekrar doldur ve sayfaya dön
            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "UrunKod", model.SecilenUrunID);
            return View(model);

        }
        // GET: Admin/UrunVaryasyonlar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var urunVaryasyon = await _context.UrunVaryasyonlari.FindAsync(id);
            if (urunVaryasyon == null)
            {
                return NotFound();
            }
            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "UrunKod", urunVaryasyon.UrunID);
            return View(urunVaryasyon);
        }

        // POST: Admin/UrunVaryasyonlar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  UrunVaryasyon urunVaryasyon)
        {
            if (id != urunVaryasyon.ID)
            {
                return NotFound();
            }
            ModelState.Remove("Urun");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(urunVaryasyon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UrunVaryasyonExists(urunVaryasyon.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "UrunKod", urunVaryasyon.UrunID);
            return View(urunVaryasyon);
        }

        // GET: Admin/UrunVaryasyonlar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var urunVaryasyon = await _context.UrunVaryasyonlari
                .Include(u => u.Urun)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (urunVaryasyon == null)
            {
                return NotFound();
            }

            return View(urunVaryasyon);
        }

        // POST: Admin/UrunVaryasyonlar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var urunVaryasyon = await _context.UrunVaryasyonlari.FindAsync(id);
            if (urunVaryasyon != null)
            {
                _context.UrunVaryasyonlari.Remove(urunVaryasyon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UrunVaryasyonExists(int id)
        {
            return _context.UrunVaryasyonlari.Any(e => e.ID == id);
        }
    }
}
