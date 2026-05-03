using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class KategorilerController : Controller
    {
        private readonly DatabaseContext _context;

        public KategorilerController(DatabaseContext context)
        {
            _context = context;
        }

        //  LİSTELEME 
        public async Task<IActionResult> Index()
        {
            var kategoriler = await _context.Kategoriler.OrderBy(k => k.SiraNo).ToListAsync();
            return View(kategoriler);
        }

        //  TEK MERKEZLİ EKLEME VE DÜZENLEME (MODAL'DAN GELEN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kaydet(Kategori model)
        {
            ModelState.Remove("UstKategori");
            ModelState.Remove("AltKategoriler");
            ModelState.Remove("Urunler");

            if (ModelState.IsValid)
            {
                if (model.ID == 0)
                {
                    _context.Kategoriler.Add(model); 
                }
                else
                {
                    _context.Kategoriler.Update(model); 
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}