using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using odevVb.Utils;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class UrunlerController : Controller
    {
        private readonly DatabaseContext _context;

        public UrunlerController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Kategoriler = await _context.Kategoriler.Where(k => k.AktifMi).OrderBy(k => k.SiraNo).ToListAsync();
            ViewBag.Markalar = await _context.Markalar.Where(m => m.AktifMi).OrderBy(m => m.MarkaAd).ToListAsync();

            //  Sayfa boş açılacak, veriyi JS çekecek.
            return View();
        }

        // ARKA PLANDA VERİ DAĞITAN AJAX METODU
        [HttpGet]
        public async Task<IActionResult> GetUrunler(string arama, int? kategoriId, int? markaId, bool? durum, int sayfa = 1, int sayfaBoyutu = 10)
        {
            var query = _context.Urunler
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .AsQueryable();

            // 1. FİLTRELEME İŞLEMLERİ (Sorgu veritabanında çalışır, RAM'i yormaz)
            if (!string.IsNullOrWhiteSpace(arama))
            {
                arama = arama.ToLower();
                query = query.Where(u => u.Baslik.ToLower().Contains(arama) ||
                                         u.UrunKod.ToLower().Contains(arama) ||
                                         (u.ModelKodu != null && u.ModelKodu.ToLower().Contains(arama)) ||
                                         (u.Renk != null && u.Renk.ToLower().Contains(arama)));
            }

            if (kategoriId.HasValue) query = query.Where(u => u.KategoriID == kategoriId.Value);
            if (markaId.HasValue) query = query.Where(u => u.MarkaID == markaId.Value);
            if (durum.HasValue) query = query.Where(u => u.AktifMi == durum.Value);

            // 2. SAYFALAMA (PAGINATION) İÇİN HESAPLAMALAR
            int toplamKayit = await query.CountAsync();
            int toplamSayfa = (int)Math.Ceiling(toplamKayit / (double)sayfaBoyutu);

            // 3. SADECE İSTENEN SAYFADAKİ KADAR VERİYİ ÇEK (Örn: 1. sayfadaki ilk 10 ürün)
            var urunler = await query
                .OrderByDescending(u => u.ID)
                .Skip((sayfa - 1) * sayfaBoyutu)
                .Take(sayfaBoyutu)
                .Select(u => new {
                    id = u.ID,
                    baslik = u.Baslik,
                    urunKod = u.UrunKod,
                    modelKodu = u.ModelKodu,
                    renk = u.Renk,
                    fiyatFormatli = u.Fiyat.ToString("C2"),
                    fiyatHam = u.Fiyat.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    aktifMi = u.AktifMi,
                    kategoriAd = u.Kategori != null ? u.Kategori.KategoriAd : "-",
                    markaAd = u.Marka != null ? u.Marka.MarkaAd : "-",
                    anaResim = u.AnaResim
                })
                .ToListAsync();

            // Veriyi JavaScript'e JSON formatında yolla
            return Json(new { urunler, toplamSayfa, mevcutSayfa = sayfa, toplamKayit });
        }

        // GET: Admin/Urunler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var urun = await _context.Urunler
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .Include(u => u.Varyasyonlar)
                .Include(u => u.Resimler) 
                .Include(u => u.Yorumlar)        
        .ThenInclude(y => y.Profil)
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
        // DİKKAT: Parametrenin sonuna "string kayitTuru" EKLENDİ!
        public async Task<IActionResult> Create(Urun urun, IFormFile? AnaResimDosyasi, IEnumerable<IFormFile>? EkGorseller, string kayitTuru)
        {
            //  GÜVENLİK ADIMI: Toplam Boyut Kontrolü
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


                if (kayitTuru == "kaydetVeYeni")
                {
                    // Başarı mesajını TempData'ya atıp formu boş haliyle tekrar açıyoruz.
                    TempData["Success"] = $"'{urun.Baslik}' başarıyla eklendi! Hız kesmeden sıradaki ürünü girebilirsiniz.";
                    return RedirectToAction(nameof(Create));
                }
                else
                {
                    // Klasik "Kaydet" dendiğinde ana listeye dönüyoruz.
                    TempData["Success"] = $"'{urun.Baslik}' başarıyla eklendi ve kataloğa alındı.";
                    return RedirectToAction(nameof(Index));
                }
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminYorumSil(int YorumID, int UrunID)
        {
            // Yorumu doğrudan ID'sine göre bul
            var silinecekYorum = await _context.Set<Yorum>().FindAsync(YorumID);

            if (silinecekYorum != null)
            {
                _context.Set<Yorum>().Remove(silinecekYorum);
                await _context.SaveChangesAsync();
            }

            // Başarıyla sildikten sonra ürünün admin detay sayfasına geri dön
            return RedirectToAction("Details", new { id = UrunID });
        }
        // POST: Admin/Urunler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            // Ürünü ve galerisindeki resimleri veritabanından çekiyoruz
            var urun = await _context.Urunler
                .Include(u => u.Resimler)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (urun != null)
            {
                // 1. ADIM: ANA RESMİ SUNUCUDAN FİZİKSEL OLARAK SİL
                if (!string.IsNullOrEmpty(urun.AnaResim))
                {
                    FileHelper.FileRemover(urun.AnaResim);
                    urun.AnaResim = string.Empty; // Veritabanındaki referansı da temizle
                }

                // 2. ADIM: GALERİDEKİ (EKSTRA) RESİMLERİ SUNUCUDAN SİL VE TABLODAN KALDIR
                if (urun.Resimler != null && urun.Resimler.Any())
                {
                    foreach (var galeriResmi in urun.Resimler)
                    {
                        FileHelper.FileRemover(galeriResmi.ResimYolu); // Fiziksel sil
                    }
                    // Resimler tablosundaki kayıtları tamamen sil 
                    _context.Resimler.RemoveRange(urun.Resimler);
                }

                // 3. ADIM: ÜRÜNÜ KÖKTEN SİLME, SADECE PASİFE AL 
                urun.AktifMi = false;

                // Sipariş geçmişinde sorun olmaması için ürünü sildirtmiyoruz, güncelliyoruz.
                _context.Urunler.Update(urun);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"'{urun.Baslik}' başarıyla arşivlendi. Görselleri sunucudan temizlendi.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UrunExists(int id)
        {
            return _context.Urunler.Any(e => e.ID == id);
        }


        [HttpPost]
        public async Task<IActionResult> HizliFiyatGuncelle([FromBody] HizliFiyatRequest request)
        {
            var urun = await _context.Urunler.FindAsync(request.Id);
            if (urun == null) return Json(new { success = false, message = "Ürün bulunamadı." });

            urun.Fiyat = request.Fiyat;
            _context.Update(urun);
            await _context.SaveChangesAsync();

          
            return Json(new { success = true, formatliFiyat = urun.Fiyat.ToString("C2") });
        }
        [HttpPost]
        public async Task<IActionResult> HizliStokGuncelle([FromBody] HizliStokRequest request)
        {
            var varyasyon = await _context.Set<UrunVaryasyon>().FindAsync(request.Id);

            if (varyasyon == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            varyasyon.StokAdedi = request.Stok;
            _context.Update(varyasyon);
            await _context.SaveChangesAsync();

            return Json(new { success = true, yeniStok = varyasyon.StokAdedi });
        }
        [HttpPost]
        public async Task<IActionResult> HizliBedenEkle([FromBody] HizliBedenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Beden))
                return Json(new { success = false, message = "Beden boş olamaz." });

            // (Opsiyonel) Aynı beden daha önce eklenmiş mi kontrolü
            var mevcutMu = await _context.Set<UrunVaryasyon>()
                .AnyAsync(v => v.UrunID == request.UrunId && v.Beden.ToLower() == request.Beden.ToLower());

            if (mevcutMu)
                return Json(new { success = false, message = "Bu ürün için bu beden zaten mevcut. Lütfen listeden stoğunu güncelleyin." });

            var yeniBeden = new UrunVaryasyon
            {
                UrunID = request.UrunId,
                Beden = request.Beden,
                StokAdedi = request.Stok
            };

            _context.Set<UrunVaryasyon>().Add(yeniBeden);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // JSON verisini karşılamak için gerekli yardımcı sınıf
        public class HizliBedenRequest
        {
            public int UrunId { get; set; }
            public string Beden { get; set; }
            public int Stok { get; set; }
        }
        public class HizliStokRequest
        {
            public int Id { get; set; }
            public int Stok { get; set; }
        }
        // Gelen JSON verisini karşılamak için küçük bir yardımcı sınıf (Controller'ın en altına veya dışına koyabilirsin)
        public class HizliFiyatRequest
        {
            public int Id { get; set; }
            public decimal Fiyat { get; set; }
        }

    }
}
