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

        // POST: Admin/Profiller/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Ad,Soyad,Mail,TelNo,Adres,Cinsiyet,Yas,Boy,Kilo,KullaniciID")] Profil profil)
        {
            // Formdan gelmeyen, sadece veritabanı ilişkisi için olan objeyi doğrulamadan çıkarıyoruz.
            ModelState.Remove("Kullanici");
            if (ModelState.IsValid)
            {
                _context.Add(profil);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KullaniciID"] = new SelectList(_context.Kullanicilar, "ID", "KullaniciAd", profil.KullaniciID);
            return View(profil);
        }

        // GET: Admin/Profiller/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profil = await _context.Profiller.FindAsync(id);
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
            var profil = await _context.Profiller.FindAsync(id);
            if (profil != null)
            {
                _context.Profiller.Remove(profil);
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
