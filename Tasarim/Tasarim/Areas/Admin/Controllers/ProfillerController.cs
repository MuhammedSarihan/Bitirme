using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class ProfillerController : Controller
    {
        private readonly DatabaseContext _context;

        public ProfillerController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Profiller
        public async Task<IActionResult> Index(string arama, int sayfa = 1)
        {
            var query = _context.Profiller.Include(p => p.Kullanici).AsQueryable();

            if (!string.IsNullOrWhiteSpace(arama))
            {
                arama = arama.ToLower();
                query = query.Where(p =>
                    p.Ad.ToLower().Contains(arama) ||
                    p.Soyad.ToLower().Contains(arama) ||
                    p.Mail.ToLower().Contains(arama) ||
                    p.TelNo.Contains(arama) ||
                    (p.Kullanici != null && p.Kullanici.KullaniciAd.ToLower().Contains(arama))
                );
            }
            else
            {
                // En yeniler en üstte
                query = query.OrderByDescending(p => p.ID);
            }

            // --- SAYFALAMA MATEMATİĞİ ---
            int sayfaBoyutu = 10;
            int toplamKayıt = await query.CountAsync(); // Filtreye uyan toplam kaç kişi var?
            int toplamSayfa = (int)Math.Ceiling(toplamKayıt / (double)sayfaBoyutu); // Kaç sayfa eder?

            // Sadece o sayfanın verilerini getir (Skip ve Take ile)
            var veriler = await query
                .Skip((sayfa - 1) * sayfaBoyutu)
                .Take(sayfaBoyutu)
                .ToListAsync();

            // View tarafında butonları çizebilmek için verileri yolluyoruz
            ViewBag.ArananKelime = arama;
            ViewBag.MevcutSayfa = sayfa;
            ViewBag.ToplamSayfa = Math.Max(1, toplamSayfa); // Hiç kayıt yoksa bile 1 sayfa görünsün

            return View(veriler);
        }

        // GET: Admin/Profiller/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profil = await _context.Profiller
                .Include(p => p.Kullanici)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (profil == null)
            {
                return NotFound();
            }

            //  E-TİCARET İSTATİSTİKLERİ HESAPLAMASI 
            if (profil.KullaniciID != null)
            {
                // Kullanıcının tüm siparişlerini çekiyoruz
                var kullaniciSiparisleri = await _context.Siparisler
                    .Where(s => s.KullaniciID == profil.KullaniciID)
                    .ToListAsync();

                // Toplam Sipariş Adedi
                ViewBag.SiparisSayisi = kullaniciSiparisleri.Count;

                // Toplam Harcama
                ViewBag.ToplamHarcama = kullaniciSiparisleri
                    .Where(s => s.SiparisDurumuID != 5)
                    .Sum(s => s.ToplamTutar);

                // Son Sipariş Tarihi
                var sonSiparis = kullaniciSiparisleri.OrderByDescending(s => s.SiparisTarihi).FirstOrDefault();
                ViewBag.SonSiparisTarihi = sonSiparis != null ? sonSiparis.SiparisTarihi.ToString("dd.MM.yyyy HH:mm") : "Sipariş Bulunmuyor";
            }
            else
            {
                ViewBag.SiparisSayisi = 0;
                ViewBag.ToplamHarcama = 0m;
                ViewBag.SonSiparisTarihi = "Hesap Yok";
            }
            // --------------------------------------------------

            return View(profil);
        }

        // GET: Admin/Profiller/Create
        public IActionResult Create()
        {
            ViewData["KullaniciID"] = new SelectList(_context.Kullanicilar, "ID", "KullaniciAd");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Profil profil)
        {
            // Model doğrulamadan "Kullanici" nesnesini çıkarıyoruz ki eksik alan yüzünden hata vermesin
            ModelState.Remove("Kullanici.ID");
            ModelState.Remove("KullaniciID");

            if (ModelState.IsValid)
            {
                // GÜVENLİK DOKUNUŞU: Bu kullanıcı adı daha önce alınmış mı kontrol edelim!
                bool kullaniciVarMi = await _context.Kullanicilar
                    .AnyAsync(k => k.KullaniciAd == profil.Kullanici.KullaniciAd);
                if (kullaniciVarMi)
                {
                    // string.Empty sayesinde hatayı direkt View'daki "All" ayarlı Kırmızı Alert kutusuna yolluyoruz.
                    ModelState.AddModelError(string.Empty, $"⚠️ DİKKAT: '{profil.Kullanici.KullaniciAd}' kullanıcı adı sistemde zaten kayıtlı. Lütfen farklı bir kullanıcı adı belirleyin.");
                    return View(profil); // Sayfayı başa sarmadan hatalı haliyle geri döndür.
                }

                // 1. Yeni Kullanıcıyı Hazırlıyoruz (Artık formdan gelen Kullanıcı Adını alıyoruz)
                Kullanici yeniKullanici = new Kullanici
                {
                    KullaniciAd = profil.Kullanici.KullaniciAd,
                    Sifre = profil.Kullanici.Sifre,
                    // Eğer istersen Profil'deki maili Kullanıcı tablosuna da kopyalayabilirsin:
                    // Mail = profil.Mail 
                };

                // 2. Kullanıcıyı Veritabanına Kaydediyoruz
                _context.Kullanicilar.Add(yeniKullanici);
                await _context.SaveChangesAsync();

                // 3. Oluşan Kullanıcının ID'sini Profil'e Bağlıyoruz
                profil.KullaniciID = yeniKullanici.ID;
                profil.Kullanici = null; // EF Core hata vermesin diye temizliyoruz

                // 4. Şimdi Profili Veritabanına Kaydediyoruz
                _context.Profiller.Add(profil);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(profil);
        }



        // GET: Admin/Profiller/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profil = await _context.Profiller
         .Include(p => p.Kullanici)
         .FirstOrDefaultAsync(p => p.ID == id);
            if (profil == null)
            {
                return NotFound();
            }
            ViewData["KullaniciID"] = new SelectList(_context.Kullanicilar, "ID", "KullaniciAd", profil.KullaniciID);
            return View(profil);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Ad,Soyad,Mail,TelNo,Adres,Cinsiyet,Yas,Boy,Kilo,KullaniciID")] Profil profil)
        {
            if (id != profil.ID)
            {
                return NotFound();
            }

            // Formdan gelmeyen objeyi doğrulamadan çıkarıyoruz
            ModelState.Remove("Kullanici");

            if (ModelState.IsValid)
            {
                // Veritabanında aynı mail adresine sahip AMA farklı ID'ye sahip (yani başkası) biri var mı?
                bool mailKullaniliyorMu = await _context.Profiller
                    .AnyAsync(p => p.Mail == profil.Mail && p.ID != profil.ID);

                if (mailKullaniliyorMu)
                {
                    // Varsa, direkt Mail inputunun altına hatayı basıyoruz
                    ModelState.AddModelError("Mail", "Bu e-posta adresi sistemde başka bir müşteri tarafından kullanılmaktadır!");

                    // Sayfa patlamasın diye kullanıcı bilgisini tekrar çekip View'a geri dönüyoruz
                    profil.Kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.ID == profil.KullaniciID);
                    return View(profil);
                }

                try
                {
                    _context.Update(profil);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfilExists(profil.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Kayıt güncellenirken bir hata oluştu.");
                }
            }

            profil.Kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.ID == profil.KullaniciID);

            return View(profil);
        }

        // GET: Admin/Profiller/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profil = await _context.Profiller
                .Include(p => p.Kullanici)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (profil == null)
            {
                return NotFound();
            }

            return View(profil);
        }

        // POST: Admin/Profiller/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profil = await _context.Profiller
        .Include(p => p.Kullanici)
        .FirstOrDefaultAsync(p => p.ID == id);
            var silinecekKullanici = profil.Kullanici;
            if (profil != null)
            {
                _context.Profiller.Remove(profil);
                if (silinecekKullanici != null)
                {
                    _context.Kullanicilar.Remove(silinecekKullanici);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfilExists(int id)
        {
            return _context.Profiller.Any(e => e.ID == id);
        }

    }
}
