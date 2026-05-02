using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SiparisController : Controller
    {
        private readonly DatabaseContext _context;

        public SiparisController(DatabaseContext context)
        {
            _context = context;
        }

        // 1. LİSTE (Açılır detaylar için Include'lar eklendi)
        public async Task<IActionResult> Index(int? durumId)
        {
            ViewBag.Durumlar = await _context.SiparisDurumlari.ToListAsync();
            ViewBag.AktifDurum = durumId;

            var query = _context.Siparisler
                .Include(s => s.SiparisDurumu)
                .Include(s => s.Kullanici).ThenInclude(k => k.Profil)
                .Include(s => s.SiparisDetaylari).ThenInclude(sd => sd.Urun).ThenInclude(u => u.Kategori) 
                .Include(s => s.SiparisDetaylari).ThenInclude(sd => sd.UrunVaryasyon) 
                .AsQueryable();

            if (durumId.HasValue)
                query = query.Where(s => s.SiparisDurumuID == durumId.Value);

            return View(await query.OrderByDescending(s => s.SiparisTarihi).ToListAsync());
        }

        // 2. DETAY EKRANI (GET) - Eksiksiz Veri Çekimi ve Katı Kurallar
        public async Task<IActionResult> Detay(int id)
        {
            var siparis = await _context.Siparisler
                .Include(s => s.SiparisDurumu)
                .Include(s => s.Kullanici).ThenInclude(k => k.Profil)
                .Include(s => s.SiparisDetaylari).ThenInclude(sd => sd.Urun)
                .Include(s => s.SiparisDetaylari).ThenInclude(sd => sd.UrunVaryasyon)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (siparis == null) return NotFound();

            //  Geçmişe dönüşü kökten engelliyoruz
            var tumDurumlar = await _context.SiparisDurumlari.ToListAsync();

            // Sadece mevcut durumdan büyük/eşit olanları VE İptal (5) seçeneğini getir
            var filtrelenmisDurumlar = tumDurumlar
                .Where(d => d.ID >= siparis.SiparisDurumuID || d.ID == 5)
                .ToList();

            // Eğer zaten iptal (5) edildiyse veya teslim (4) edildiyse listeyi kilitle
            if (siparis.SiparisDurumuID == 5)
            {
                filtrelenmisDurumlar = tumDurumlar.Where(d => d.ID == 5).ToList();
            }
            else if (siparis.SiparisDurumuID == 4)
            {
                filtrelenmisDurumlar = tumDurumlar.Where(d => d.ID == 4).ToList();
            }

            ViewBag.Durumlar = new SelectList(filtrelenmisDurumlar, "ID", "DurumAd", siparis.SiparisDurumuID);

            return View(siparis);
        }
        // 3. DETAY EKRANI SAĞ TARAF FORM KAYDETME (POST)
        [HttpPost]
        public async Task<IActionResult> DurumGuncelle(int id, int SiparisDurumuID, string? KargoNo)
        {
            var siparis = await _context.Siparisler
                .Include(s => s.SiparisDetaylari)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (siparis == null) return NotFound();

            // Teslim edilmiş veya iptal edilmiş sipariş form üzerinden zorlanarak gönderilirse engelle
            if (siparis.SiparisDurumuID == 4 || siparis.SiparisDurumuID == 5)
            {
                TempData["Error"] = "Kilitli siparişlerde durum güncellemesi yapılamaz.";
                return RedirectToAction("Detay", new { id = siparis.ID });
            }

            // İptal (5) ediliyorsa stokları geri iade et
            if (SiparisDurumuID == 5 && siparis.SiparisDurumuID != 5)
            {
                foreach (var detay in siparis.SiparisDetaylari)
                {
                    var varyasyon = await _context.UrunVaryasyonlari.FindAsync(detay.UrunVaryasyonID);
                    if (varyasyon != null) varyasyon.StokAdedi += detay.Adet;
                }
            }

            siparis.SiparisDurumuID = SiparisDurumuID;

            // Kargoya verildiyse (3) Kargo Numarasını kaydet
            if (SiparisDurumuID == 3 && !string.IsNullOrEmpty(KargoNo))
            {
                siparis.KargoNo = KargoNo;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Sipariş durumu başarıyla güncellendi.";

            return RedirectToAction("Detay", new { id = siparis.ID });
        }

        // 3. HIZLI VE KATI DURUM GÜNCELLEME
        [HttpPost]
        public async Task<IActionResult> HizliDurumGuncelle(int id, int yeniDurumId)
        {
            var siparis = await _context.Siparisler.Include(s => s.SiparisDetaylari).FirstOrDefaultAsync(s => s.ID == id);
            if (siparis == null) return Json(new { success = false, message = "Sipariş bulunamadı." });

            // KURAL 1: İptal edilen (5) bir sipariş ASLA geri döndürülemez.
            if (siparis.SiparisDurumuID == 5)
                return Json(new { success = false, message = "İptal edilen sipariş geri alınamaz!" });

            // KURAL 2: Teslim edilen (4) bir sipariş iptal edilemez veya geriye alınamaz.
            if (siparis.SiparisDurumuID == 4)
                return Json(new { success = false, message = "Teslim edilmiş sipariş değiştirilemez!" });

            // KURAL 3: Süreç geriye doğru işleyemez (Örn: Kargoya verilen, Onay bekliyora dönemez). 
            // Sadece İptal (5) durumu ileri yöndedir ve her zaman seçilebilir.
            if (yeniDurumId < siparis.SiparisDurumuID && yeniDurumId != 5)
                return Json(new { success = false, message = "Sipariş durumu geriye doğru alınamaz!" });

            // KURAL 4: Eğer sipariş YENİ İPTAL ediliyorsa stokları geri yükle.
            if (yeniDurumId == 5 && siparis.SiparisDurumuID != 5)
            {
                foreach (var detay in siparis.SiparisDetaylari)
                {
                    var varyasyon = await _context.UrunVaryasyonlari.FindAsync(detay.UrunVaryasyonID);
                    if (varyasyon != null) varyasyon.StokAdedi += detay.Adet;
                }
            }

            siparis.SiparisDurumuID = yeniDurumId;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 4. SADECE ÇIKTI (FİŞ) İÇİN ÖZEL SAYFA
        public async Task<IActionResult> FisYazdir(int id)
        {
            var siparis = await _context.Siparisler
                .Include(s => s.SiparisDurumu)
                .Include(s => s.Kullanici).ThenInclude(k => k.Profil)
                .Include(s => s.SiparisDetaylari).ThenInclude(sd => sd.Urun)
                .Include(s => s.SiparisDetaylari).ThenInclude(sd => sd.UrunVaryasyon)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (siparis == null) return NotFound();

            // Layout kullanmamak için PartialView veya boş bir View döneceğiz
            return View(siparis);
        }
    }
}