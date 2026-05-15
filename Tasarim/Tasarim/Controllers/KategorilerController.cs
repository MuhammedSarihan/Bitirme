using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasarim.Data;

namespace Tasarim.Controllers
{
    public class KategorilerController : Controller
    {
        private readonly DatabaseContext _context;

        public KategorilerController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id) // IndexAsync yerine Index kullanmak ASP.NET Core yönlendirmeleri için daha sağlıklıdır
        {
            if (id == null) return NotFound();

            // 1. Önce sadece kategorinin kendisini buluyoruz
            var kategori = await _context.Kategoriler.FirstOrDefaultAsync(m => m.ID == id);

            if (kategori == null) return NotFound();

            // 2. Bu kategoriye ve bunun ALT KATEGORİLERİNE ait ID'leri buluyoruz
            // (Eğer veritabanındaki adın UstKategoriID değilse burayı düzeltmeyi unutma)
            var kategoriIdListesi = await _context.Kategoriler
                .Where(k => k.ID == id || k.UstKategoriID == id)
                .Select(k => k.ID)
                .ToListAsync();

            // 3. Sadece bu ID'lere sahip olan AKTİF ürünleri çekiyoruz
            var urunler = await _context.Urunler
                .Include(u => u.Varyasyonlar) // "Tükendi" hesaplaması için ekledik
                .Include(u => u.Yorumlar)     // Yıldız değerlendirmesi için ekledik
                .Include(u => u.KampanyaUrunleri) 
                    .ThenInclude(ku => ku.Kampanya)
                .Where(u => u.AktifMi == true && kategoriIdListesi.Contains(u.KategoriID))
                .OrderByDescending(u => u.ID)
                .ToListAsync();

            kategori.Urunler = urunler;

            return View(kategori);
        }
    }
}