using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevVb.Utils;
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.Areas.Admin.Models;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class KampanyalarController : Controller
    {
        private readonly DatabaseContext _context;

        public KampanyalarController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Kampanyalar
        public async Task<IActionResult> Index()
        {
            return View(await _context.Kampanyalar.ToListAsync());
        }

        // GET: Admin/Kampanyalar/Details/
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kampanya = await _context.Kampanyalar
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kampanya == null)
            {
                return NotFound();
            }

            return View(kampanya);
        }

        // GET: Admin/Kampanyalar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Kampanyalar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Kampanya kampanya, IFormFile? AnaResimDosyasi)
        {
            if (ModelState.IsValid)
            {
                if (AnaResimDosyasi != null)
                {
                    kampanya.KampanyaResmi = await FileHelper.FileLoaderAsync(AnaResimDosyasi);
                }
                _context.Add(kampanya);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kampanya);
        }

        // GET: Admin/Kampanyalar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kampanya = await _context.Kampanyalar.FindAsync(id);
            if (kampanya == null)
            {
                return NotFound();
            }
            return View(kampanya);
        }

        // POST: Admin/Kampanyalar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Kampanya kampanya, IFormFile? AnaResimDosyasi, bool cbResmiSil = false)
        {
            if (id != kampanya.ID)
            {
                return NotFound();
            }
            ModelState.Remove("KampanyaResmi");

            if (ModelState.IsValid)
            {
                try
                {
                    if (cbResmiSil)
                    {
                        kampanya.KampanyaResmi = string.Empty;
                    }

                    else if (AnaResimDosyasi != null)
                    {
                        kampanya.KampanyaResmi = await FileHelper.FileLoaderAsync(AnaResimDosyasi);
                    }
                    _context.Update(kampanya);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KampanyaExists(kampanya.ID))
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
            return View(kampanya);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var kampanya = await _context.Kampanyalar
                .Include(k => k.KampanyaUrunleri)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (kampanya != null)
            {
                // Varsa köprü tablosunu sil
                if (kampanya.KampanyaUrunleri != null && kampanya.KampanyaUrunleri.Any())
                {
                    _context.KampanyaUrunleri.RemoveRange(kampanya.KampanyaUrunleri);
                }

                // Varsa resmi sil
                if (!string.IsNullOrEmpty(kampanya.KampanyaResmi))
                {
                    FileHelper.FileRemover(kampanya.KampanyaResmi);
                }

                // Kampanyayı sil
                _context.Kampanyalar.Remove(kampanya);
                await _context.SaveChangesAsync();
            }

            // İşlem bitince listeye geri dön
            return RedirectToAction(nameof(Index));
        }

        //Kampanyaya Ürünler Atama İşlemi
        public async Task<IActionResult> UrunAta(int id)
        {
            var kampanya = await _context.Kampanyalar
                .Include(k => k.KampanyaUrunleri)
                .FirstOrDefaultAsync(k => k.ID == id);

            if (kampanya == null) return NotFound();

            // DİKKAT: Kategori ve Marka tablolarını da Include ile çekiyoruz
            var butunUrunler = await _context.Urunler
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .ToListAsync();

            var model = new List<KampanyaUrunViewModel>();

            // Filtre dropdownları için benzersiz kategori ve markaları topluyoruz
            var kategoriler = new List<string>();
            var markalar = new List<string>();

            foreach (var urun in butunUrunler)
            {
                string katAd = urun.Kategori != null ? urun.Kategori.KategoriAd : "Kategorisiz";
                string marAd = urun.Marka != null ? urun.Marka.MarkaAd : "Markasız";

                // Dropdown için listeleri dolduruyoruz
                if (!kategoriler.Contains(katAd)) kategoriler.Add(katAd);
                if (!markalar.Contains(marAd)) markalar.Add(marAd);

                model.Add(new KampanyaUrunViewModel
                {
                    UrunID = urun.ID,
                    UrunKod = urun.UrunKod,
                    UrunAd = urun.Aciklama,
                    KategoriAd = katAd,
                    MarkaAd = marAd,
                    // Eğer ürünün ana resmi 'AnaResim' gibi bir property ise buraya ekle, yoksa boş bırak
                    ResimUrl = urun.AnaResim,
                    SeciliMi = kampanya.KampanyaUrunleri.Any(ku => ku.UrunID == urun.ID)
                });
            }

            ViewBag.KampanyaAd = kampanya.KampanyaAd;
            ViewBag.KampanyaID = kampanya.ID;
            ViewBag.Kategoriler = kategoriler;
            ViewBag.Markalar = markalar;

            return View(model);
        }

        //(POST): Formdan gelen seçili ürünleri veritabanına kaydetme
        [HttpPost]
        public async Task<IActionResult> UrunAta(int kampanyaId, List<int> secilenUrunler)
        {
            // 1. Önce bu kampanyanın eski ürün kayıtlarını köprü tablosundan tamamen temizle
            var silinecekEskiUrunler = _context.KampanyaUrunleri.Where(ku => ku.KampanyaID == kampanyaId);
            _context.KampanyaUrunleri.RemoveRange(silinecekEskiUrunler);

            // 2. Formdan seçilip gelen (Checkbox'ı işaretli) yeni ürünleri köprü tablosuna ekle
            if (secilenUrunler != null && secilenUrunler.Any())
            {
                foreach (var urunId in secilenUrunler)
                {
                    _context.KampanyaUrunleri.Add(new KampanyaUrunleri
                    {
                        KampanyaID = kampanyaId,
                        UrunID = urunId
                    });
                }
            }

            // Değişiklikleri veritabanına yansıt
            await _context.SaveChangesAsync();

            // İşlem bitince listeye geri dön
            return RedirectToAction("Index");
        }
        private bool KampanyaExists(int id)
        {
            return _context.Kampanyalar.Any(e => e.ID == id);
        }
    }
}
