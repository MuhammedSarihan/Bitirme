using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class MarkalarController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IWebHostEnvironment _env; // Resim yüklemek için ortam bilgisini alıyoruz

        public MarkalarController(DatabaseContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            var markalar = await _context.Markalar.ToListAsync();
            return View(markalar);
        }

        // 2. TEK MERKEZLİ EKLEME, DÜZENLEME VE RESİM YÜKLEME
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kaydet(Marka model, IFormFile? YuklenenLogo, bool LogoyuSil)
        {
            ModelState.Remove("Urunler");

            if (ModelState.IsValid)
            {
                Marka? mevcutMarka = null;
                if (model.ID != 0)
                {
                    mevcutMarka = await _context.Markalar.AsNoTracking().FirstOrDefaultAsync(m => m.ID == model.ID);
                }

                // 1. ADIM: EĞER KULLANICI "MEVCUT LOGOYU SİL" KUTUSUNU İŞARETLEDİYSE
                if (LogoyuSil && mevcutMarka != null)
                {
                    // FİZİKSEL SİLME İŞLEMİ (Eski dosyayı sunucudan temizle)
                    if (!string.IsNullOrEmpty(mevcutMarka.Logo))
                    {
                        string silinecekYol = Path.Combine(_env.WebRootPath, "img", mevcutMarka.Logo);
                        if (System.IO.File.Exists(silinecekYol))
                        {
                            System.IO.File.Delete(silinecekYol);
                        }
                    }

                    model.Logo = null; // Veritabanından da adını sil
                }
                // Silinmediyse ve yeni resim de yoksa, eski resmi koru
                else if (mevcutMarka != null && YuklenenLogo == null)
                {
                    model.Logo = mevcutMarka.Logo;
                }

                // 2. ADIM: YENİ BİR RESİM YÜKLENDİYSE
                if (YuklenenLogo != null && YuklenenLogo.Length > 0)
                {
                    // FİZİKSEL SİLME İŞLEMİ (Eğer markanın zaten eski bir logosu varsa, üstüne yeni yüklendiği için eskisini çöpe at)
                    if (mevcutMarka != null && !string.IsNullOrEmpty(mevcutMarka.Logo) && !LogoyuSil)
                    {
                        string eskiYol = Path.Combine(_env.WebRootPath, "img", mevcutMarka.Logo);
                        if (System.IO.File.Exists(eskiYol))
                        {
                            System.IO.File.Delete(eskiYol);
                        }
                    }

                    // Yeni dosyayı kaydet
                    string dosyaUzantisi = Path.GetExtension(YuklenenLogo.FileName);
                    string yeniDosyaAdi = "marka_" + DateTime.Now.Ticks.ToString() + dosyaUzantisi;
                    string kayitYolu = Path.Combine(_env.WebRootPath, "img", yeniDosyaAdi);

                    using (var stream = new FileStream(kayitYolu, FileMode.Create))
                    {
                        await YuklenenLogo.CopyToAsync(stream);
                    }

                    model.Logo = yeniDosyaAdi;
                }

                // 3. ADIM: VERİTABANINA KAYDET VEYA GÜNCELLE
                if (model.ID == 0)
                {
                    _context.Markalar.Add(model);
                }
                else
                {
                    _context.Markalar.Update(model);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}