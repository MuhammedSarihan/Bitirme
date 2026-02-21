using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using odevVb.Utils;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UrunlerController : Controller
    {
        private readonly DatabaseContext _context;

        public UrunlerController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Urunler
        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.Urunler.Include(u => u.Kategori).Include(u => u.Marka);
            return View(await databaseContext.ToListAsync());
        }

        // GET: Admin/Urunler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var urun = await _context.Urunler
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .Include(u => u.Varyasyonlar)
                .Include(u => u.Resimler) // DETAYDA GALERİYİ GÖRMEK İÇİN EKLENDİ
                .FirstOrDefaultAsync(m => m.ID == id);

            if (urun == null) return NotFound();

            return View(urun);
        }

        // GET: Admin/Urunler/Create
        public IActionResult Create()
        {
            ViewData["KategoriID"] = new SelectList(_context.Kategoriler, "ID", "KategoriAd");
            ViewData["MarkaID"] = new SelectList(_context.Markalar, "ID", "MarkaAd");
            return View();
        }

        // POST: Admin/Urunler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // List<IFormFile> EkGorseller EKLENDİ!
        public async Task<IActionResult> Create(Urun urun, IFormFile? AnaResimDosyasi, IEnumerable<IFormFile>? EkGorseller)
        {
            // 1. GÜVENLİK ADIMI: Toplam Boyut Kontrolü
            long toplamBoyut = EkGorseller?.Sum(f => f.Length) ?? 0;
            if (toplamBoyut > 20971520) // 20 MB
            {
                ModelState.AddModelError("", "Yüklemeye çalıştığınız dosyaların toplam boyutu 20MB sınırını aşıyor.");
                ViewData["KategoriID"] = new SelectList(_context.Kategoriler, "ID", "KategoriAd", urun.KategoriID);
                ViewData["MarkaID"] = new SelectList(_context.Markalar, "ID", "MarkaAd", urun.MarkaID);
                return View(urun);
            }
            ModelState.Remove("Kategori");
            ModelState.Remove("Marka");
            ModelState.Remove("Varyasyonlar");
            ModelState.Remove("Resimler");
            ModelState.Remove("Yorumlar");
            ModelState.Remove("Favoriler");
            ModelState.Remove("SepetItems");
            ModelState.Remove("SiparisDetaylari");
            ModelState.Remove("LLSonuc");

            if (ModelState.IsValid)
            {
                if (AnaResimDosyasi != null)
                {
                    urun.AnaResim = await FileHelper.FileLoaderAsync(AnaResimDosyasi);
                }

                _context.Add(urun);
                await _context.SaveChangesAsync(); // Ürün kaydedilir ve urun.ID oluşur!

                // ÜRÜN OLUŞTUKTAN SONRA ÇOKLU RESİMLERİ YÜKLE
                if (EkGorseller != null && EkGorseller.Any())
                {
                    int sira = 1;
                    foreach (var dosya in EkGorseller)
                    {
                        var yol = await FileHelper.FileLoaderAsync(dosya);
                        _context.Resimler.Add(new Resim
                        {
                            UrunID = urun.ID,
                            ResimYolu = yol,
                            SiraNo = sira++
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["KategoriID"] = new SelectList(_context.Kategoriler, "ID", "KategoriAd", urun.KategoriID);
            ViewData["MarkaID"] = new SelectList(_context.Markalar, "ID", "MarkaAd", urun.MarkaID);
            return View(urun);
        }

        // GET: Admin/Urunler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Edit ekranında galeriyi göstermek için .Include(u => u.Resimler) ekledik
            var urun = await _context.Urunler
                .Include(u => u.Resimler)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (urun == null) return NotFound();

            ViewData["KategoriID"] = new SelectList(_context.Kategoriler, "ID", "KategoriAd", urun.KategoriID);
            ViewData["MarkaID"] = new SelectList(_context.Markalar, "ID", "MarkaAd", urun.MarkaID);
            return View(urun);
        }

        // POST: Admin/Urunler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // List<IFormFile> EkGorseller EKLENDİ!
        public async Task<IActionResult> Edit(int id, Urun urun, IFormFile? AnaResimDosyasi, IEnumerable<IFormFile>? EkGorseller, bool cbResmiSil = false)
        {
            if (id != urun.ID) return NotFound();

            ModelState.Remove("Kategori");
            ModelState.Remove("Marka");
            ModelState.Remove("Varyasyonlar");
            ModelState.Remove("Resimler");
            ModelState.Remove("Yorumlar");
            ModelState.Remove("Favoriler");
            ModelState.Remove("SepetItems");
            ModelState.Remove("SiparisDetaylari");
            ModelState.Remove("LLSonuc");

            if (ModelState.IsValid)
            {
                try
                {
                    if (cbResmiSil)
                    {
                        urun.AnaResim = string.Empty;
                    }

                    if (AnaResimDosyasi != null)
                    {
                        urun.AnaResim = await FileHelper.FileLoaderAsync(AnaResimDosyasi);
                    }

                    _context.Update(urun);

                    // YENİ EKLENEN EKSTRALAR VARSA ONLARI DA GALERİYE İLAVE ET
                    if (EkGorseller != null && EkGorseller.Any())
                    {
                        // Mevcut resimlerin en büyük sıra numarasını bulalım ki üstüne eklesin
                        int maxSira = _context.Resimler.Where(r => r.UrunID == urun.ID).Max(r => (int?)r.SiraNo) ?? 0;

                        foreach (var dosya in EkGorseller)
                        {
                            maxSira++;
                            var yol = await FileHelper.FileLoaderAsync(dosya);
                            _context.Resimler.Add(new Resim
                            {
                                UrunID = urun.ID,
                                ResimYolu = yol,
                                SiraNo = maxSira
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UrunExists(urun.ID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["KategoriID"] = new SelectList(_context.Kategoriler, "ID", "KategoriAd", urun.KategoriID);
            ViewData["MarkaID"] = new SelectList(_context.Markalar, "ID", "MarkaAd", urun.MarkaID);
            return View(urun);
        }

        // YENİ METOT: Galeriden Tek Bir Ekstra Resmi Silmek İçin (Edit sayfasından tetiklenir)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EkResimSil(int resimId, int urunId)
        {
            var resim = await _context.Resimler.FindAsync(resimId);
            if (resim != null)
            {
                FileHelper.FileRemover(resim.ResimYolu); // Fiziksel sil
                _context.Resimler.Remove(resim);         // Veritabanından sil
                await _context.SaveChangesAsync();
            }
            // Ürünün Edit sayfasına geri dön
            return RedirectToAction(nameof(Edit), new { id = urunId });
        }

        // GET: Admin/Urunler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var urun = await _context.Urunler
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (urun == null) return NotFound();

            return View(urun);
        }

        // POST: Admin/Urunler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var urun = await _context.Urunler
                .Include(u => u.Resimler) // Ürünü silerken galerisini de getirelim ki onları da çöpe atalım
                .FirstOrDefaultAsync(u => u.ID == id);

            if (urun != null)
            {
                // Ana resmi sil
                if (!string.IsNullOrEmpty(urun.AnaResim)) FileHelper.FileRemover(urun.AnaResim);

                // Varsa galerideki tüm ekstra resimleri fiziksel olarak sil
                if (urun.Resimler != null)
                {
                    foreach (var galeriResmi in urun.Resimler)
                    {
                        FileHelper.FileRemover(galeriResmi.ResimYolu);
                    }
                }

                _context.Urunler.Remove(urun);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UrunExists(int id)
        {
            return _context.Urunler.Any(e => e.ID == id);
        }
    }
}
