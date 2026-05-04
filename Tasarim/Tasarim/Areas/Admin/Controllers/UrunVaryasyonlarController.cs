using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class UrunVaryasyonlarController : Controller
    {
        private readonly DatabaseContext _context;

        public UrunVaryasyonlarController(DatabaseContext context)
        {
            _context = context;
        }

        // Toplu Stok Ekleme Sayfasını Aç (GET)
        [HttpGet]
        public IActionResult Create()
        {
            // Dropdown için ürünleri gönderiyoruz
            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "UrunKod");

            var model = new Models.TopluVaryasyonViewModel();
            model.Varyasyonlar = new List<UrunVaryasyon>();

            return View(model);
        }

        //  Eklenen Stokları Veritabanına Kaydet/Güncelle (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.TopluVaryasyonViewModel model)
        {
            // Sadece "Beden" kısmı doldurulmuş ve stoğu 0'dan büyük olan satırları ayıklıyoruz
            var eklenecekVaryasyonlar = model.Varyasyonlar
                .Where(v => !string.IsNullOrWhiteSpace(v.Beden) && v.StokAdedi > 0)
                .ToList();

            // Ürün seçilmişse ve en az 1 tane geçerli beden girilmişse işlemleri yap
            if (model.SecilenUrunID > 0 && eklenecekVaryasyonlar.Any())
            {
                foreach (var varyasyon in eklenecekVaryasyonlar)
                {
                    // 1. ADIM: Veritabanında aynı Ürün ve aynı Beden var mı?
                    var varolanVaryasyon = await _context.UrunVaryasyonlari
                        .FirstOrDefaultAsync(v => v.UrunID == varyasyon.UrunID && v.Beden == varyasyon.Beden);

                    if (varolanVaryasyon != null)
                    {
                        // VARSA : Sadece mevcut stoğun üzerine ekle (Güncelleme İşlemi)
                        varolanVaryasyon.StokAdedi += varyasyon.StokAdedi;
                        _context.Update(varolanVaryasyon);
                    }
                    else
                    {
                        // YOKSA: Yeni varyasyon olarak kaydet (Ekleme İşlemi)
                        _context.Add(varyasyon);
                    }
                }

                // Tüm liste döngüden çıktıktan sonra tek seferde kaydet
                await _context.SaveChangesAsync();

                // İşlem bitince yeni tasarladığımız Ürün Kataloğuna geri dön!
                return RedirectToAction("Index", "Urunler");
            }

            // Hata veya eksik giriş varsa listeyi tekrar doldur ve sayfaya dön
            ViewData["UrunID"] = new SelectList(_context.Urunler, "ID", "UrunKod", model.SecilenUrunID);
            return View(model);
        }
    }
}