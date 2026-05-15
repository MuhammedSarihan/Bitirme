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


        public async Task<IActionResult> Index()
        {
            var sliderlar = await _context.Sliders.OrderBy(s => s.SiraNo).ToListAsync();
            return View(sliderlar);
        }


        [HttpGet]
        public async Task<IActionResult> GetSlider(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null) return NotFound();
            return Json(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Slider model, IFormFile? resimDosyasi)
        {
            try
            {
                // YENİ EKLEME İŞLEMİ
                if (model.ID == 0)
                {
                    if (resimDosyasi != null)
                    {
                        model.SliderResim = await FileHelper.FileLoaderAsync(resimDosyasi);
                    }
                    _context.Sliders.Add(model);
                }
                // GÜNCELLEME İŞLEMİ
                else
                {
                    var mevcutSlider = await _context.Sliders.FindAsync(model.ID);
                    if (mevcutSlider == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

                    // Eğer yeni resim yüklendiyse
                    if (resimDosyasi != null)
                    {
                        // ESKİ RESMİ SUNUCUDAN SİL (Çöp birikmesini önle)
                        if (!string.IsNullOrEmpty(mevcutSlider.SliderResim))
                        {
                            FileHelper.FileRemover(mevcutSlider.SliderResim);
                        }

                        mevcutSlider.SliderResim = await FileHelper.FileLoaderAsync(resimDosyasi);
                    }

                    mevcutSlider.Baslik = model.Baslik;
                    mevcutSlider.SliderAciklama = model.SliderAciklama;
                    mevcutSlider.Link = model.Link;
                    mevcutSlider.SiraNo = model.SiraNo;

                    _context.Update(mevcutSlider);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSlider(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider != null)
            {
                // SUNUCUDAN GÖRSELİ KALICI OLARAK SİL
                if (!string.IsNullOrEmpty(slider.SliderResim))
                {
                    FileHelper.FileRemover(slider.SliderResim);
                }

                _context.Sliders.Remove(slider);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}