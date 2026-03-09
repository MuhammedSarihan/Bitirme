using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevVb.Utils;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class SliderlarController : Controller
    {
        private readonly DatabaseContext _context;

        public SliderlarController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Sliderlar
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sliders.ToListAsync());
        }

        // GET: Admin/Sliderlar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.ID == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // GET: Admin/Sliderlar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Sliderlar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Slider slider, IFormFile? AnaResimDosyasi)
        {
            ModelState.Remove("SliderResim");
            if (ModelState.IsValid)
            {
                if (AnaResimDosyasi != null)
                {
                    slider.SliderResim = await FileHelper.FileLoaderAsync(AnaResimDosyasi);
                }
                _context.Add(slider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        // GET: Admin/Sliderlar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        // POST: Admin/Sliderlar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Slider slider, IFormFile? AnaResimDosyasi, bool cbResmiSil = false)
        {
            if (id != slider.ID)
            {
                return NotFound();
            }

            ModelState.Remove("SliderResim");
            if (ModelState.IsValid)
            {
                try
                {
                    if (cbResmiSil)
                    {
                        slider.SliderResim = string.Empty;
                    }

                    if (AnaResimDosyasi != null)
                    {
                        slider.SliderResim = await FileHelper.FileLoaderAsync(AnaResimDosyasi);
                    }
                    _context.Update(slider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SliderExists(slider.ID))
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
            return View(slider);
        }

        // GET: Admin/Sliderlar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.ID == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // POST: Admin/Sliderlar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider != null)
            {
                if (!string.IsNullOrEmpty(slider.SliderResim)) FileHelper.FileRemover(slider.SliderResim);

                _context.Sliders.Remove(slider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SliderExists(int id)
        {
            return _context.Sliders.Any(e => e.ID == id);
        }
    }
}
