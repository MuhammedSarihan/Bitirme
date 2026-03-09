using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevVb.Utils;
using Tasarim.Core.Entities;
using Tasarim.Data;

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

        // GET: Admin/Kampanyalar/Details/5
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

        // GET: Admin/Kampanyalar/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Admin/Kampanyalar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kampanya = await _context.Kampanyalar.FindAsync(id);
            if (kampanya != null)
            {
                if (!string.IsNullOrEmpty(kampanya.KampanyaResmi)) FileHelper.FileRemover(kampanya.KampanyaResmi);
                _context.Kampanyalar.Remove(kampanya);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KampanyaExists(int id)
        {
            return _context.Kampanyalar.Any(e => e.ID == id);
        }
    }
}
