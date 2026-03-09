using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class KullanicilarController : Controller
    {
        private readonly DatabaseContext _context;

        public KullanicilarController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Kullanicilar
        public async Task<IActionResult> Index()
        {
            return View(await _context.Kullanicilar.ToListAsync());
        }

        // GET: Admin/Kullanicilar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kullanici == null)
            {
                return NotFound();
            }

            return View(kullanici);
        }

        // GET: Admin/Kullanicilar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Kullanicilar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,KullaniciAd,Sifre,AdminMi")] Kullanici kullanici)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kullanici);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kullanici);
        }

        // GET: Admin/Kullanicilar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }
            return View(kullanici);
        }

        // POST: Admin/Kullanicilar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,KullaniciAd,Sifre,AdminMi")] Kullanici kullanici)
        {
            if (id != kullanici.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kullanici);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KullaniciExists(kullanici.ID))
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
            return View(kullanici);
        }

        // GET: Admin/Kullanicilar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kullanici == null)
            {
                return NotFound();
            }

            return View(kullanici);
        }

        // POST: Admin/Kullanicilar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici != null)
            {
                _context.Kullanicilar.Remove(kullanici);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KullaniciExists(int id)
        {
            return _context.Kullanicilar.Any(e => e.ID == id);
        }
    }
}
