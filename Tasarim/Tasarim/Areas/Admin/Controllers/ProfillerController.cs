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
        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.Profiller.Include(p => p.Kullanici);
            return View(await databaseContext.ToListAsync());
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


        // POST: Admin/Profiller/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("ID,Ad,Soyad,Mail,TelNo,Adres,Cinsiyet,Yas,Boy,Kilo,KullaniciID")] Profil profil)
        //{
        //    // Formdan gelmeyen, sadece veritabanı ilişkisi için olan objeyi doğrulamadan çıkarıyoruz.
        //    ModelState.Remove("Kullanici");
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(profil);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["KullaniciID"] = new SelectList(_context.Kullanicilar, "ID", "KullaniciAd", profil.KullaniciID);
        //    return View(profil);
        //}

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

        // POST: Admin/Profiller/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Ad,Soyad,Mail,TelNo,Adres,Cinsiyet,Yas,Boy,Kilo,KullaniciID")] Profil profil)
        {
            if (id != profil.ID)
            {
                return NotFound();
            }
            // Formdan gelmeyen, sadece veritabanı ilişkisi için olan objeyi doğrulamadan çıkarıyoruz.
            ModelState.Remove("Kullanici");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profil);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["KullaniciID"] = new SelectList(_context.Kullanicilar, "ID", "KullaniciAd", profil.KullaniciID);
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
