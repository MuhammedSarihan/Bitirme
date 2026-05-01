using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // User.FindFirstValue için gerekli
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.Models;

// ÖNEMLİ: Sadece giriş yapmış kullanıcılar ödeme aşamasına geçebilir
[Authorize]
public class SiparisController : Controller
{
    private readonly DatabaseContext _context;

    public SiparisController(DatabaseContext context)
    {
        _context = context;
    }

    // 1. ÖDEME SAYFASINI AÇ
    [HttpGet]
    public async Task<IActionResult> Odeme()
    {
        // 1. Giriş yapmış kullanıcının ID'sini Session veya Claim'den al (Projedeki Auth yapına göre)
        // Eğer Claim bazlı Cookie Authentication kullanıyorsan:
        string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
        {
            return RedirectToAction("Login", "Kullanici");
        }

        // 2. Kullanıcının PROFIL bilgilerini çek
        var profil = await _context.Profiller
                                   .FirstOrDefaultAsync(p => p.KullaniciID == currentUserId);

        // Profil yoksa (Kullanıcı kayıt olmuş ama profil doldurmamışsa)
        if (profil == null)
        {
            // Kullanıcıyı profilini doldurması için yönlendirebilirsin
            return RedirectToAction("ProfilDuzenle", "Kullanici");
        }

        // 3. Sepeti, Ürünleri ve KAMPANYALARI ile birlikte çek
        // KampanyaliFiyat'ın hesaplanabilmesi için KampanyaUrunleri ve Kampanya tablolarını Include etmek ZORUNLUDUR!
        var sepet = await _context.Sepetler
            .Include(s => s.SepetItems)
                .ThenInclude(si => si.UrunVaryasyon)
                    .ThenInclude(uv => uv.Urun)
                        .ThenInclude(u => u.KampanyaUrunleri) // Kampanyalı fiyat için şart
                            .ThenInclude(ku => ku.Kampanya)   // Kampanyalı fiyat için şart
            .FirstOrDefaultAsync(s => s.KullaniciID == currentUserId);

        if (sepet == null || sepet.SepetItems == null || !sepet.SepetItems.Any())
        {
            return RedirectToAction("Index", "Sepet");
        }

        // 4. İndirimli Gerçek Tutarı Hesapla (KampanyaliFiyat kullanılıyor!)
        decimal gercekSepetToplami = sepet.SepetItems.Sum(item => item.Adet * item.UrunVaryasyon.Urun.KampanyaliFiyat);

        var model = new OdemeViewModel
        {
            // PROFIL tablosundan gelen veriler
            AdSoyad = profil.Ad + " " + profil.Soyad,
            Telefon = profil.TelNo,
            TeslimatAdresi = profil.Adres,
            OdenecekTutar = gercekSepetToplami
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Odeme(OdemeViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            return RedirectToAction("Login", "Kullanici");

        var sepet = await _context.Sepetler
            .Include(s => s.SepetItems)
                .ThenInclude(si => si.UrunVaryasyon)
                    .ThenInclude(uv => uv.Urun)
                        .ThenInclude(u => u.KampanyaUrunleri)
                            .ThenInclude(ku => ku.Kampanya)
            .FirstOrDefaultAsync(s => s.KullaniciID == currentUserId);

        if (sepet == null || !sepet.SepetItems.Any()) return RedirectToAction("Index", "Sepet");

        decimal gercekTutar = sepet.SepetItems.Sum(item => item.Adet * item.UrunVaryasyon.Urun.KampanyaliFiyat);
        string iyzicoIslemNo = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(); // Simülasyon

        // --- 1. SİHİRLİ DOKUNUŞ: TRANSACTION BAŞLIYOR ---
        // Eğer aşağıdaki işlemlerin BİRİ bile hata verirse, veritabanına hiçbir şey yazılmaz!
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var yeniSiparis = new Siparis
            {
                SiparisNo = "SP-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                SiparisTarihi = DateTime.Now,
                ToplamTutar = gercekTutar,
                KullaniciID = currentUserId,
                SiparisDurumuID = 1,
                TeslimatAdresi = model.TeslimatAdresi
            };
            _context.Siparisler.Add(yeniSiparis);
            await _context.SaveChangesAsync();

            foreach (var item in sepet.SepetItems)
            {
                var detay = new SiparisDetay
                {
                    SiparisID = yeniSiparis.ID,
                    UrunID = item.UrunVaryasyon.UrunID,
                    UrunVaryasyonID = item.UrunVaryasyonID,
                    Adet = item.Adet,
                    BirimFiyat = item.UrunVaryasyon.Urun.KampanyaliFiyat
                };
                _context.SiparisDetaylari.Add(detay);

                // STOK DÜŞME VE KONTROL
                if (item.UrunVaryasyon.StokAdedi >= item.Adet)
                {
                    item.UrunVaryasyon.StokAdedi -= item.Adet;
                }
                else
                {
                    // Stok yetersizse işlemi anında iptal et (Rollback)
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"{item.UrunVaryasyon.Urun.Baslik} ürünü için stok yetersiz! (Kalan: {item.UrunVaryasyon.StokAdedi})");
                    return View(model);
                }
            }

            var yeniOdeme = new Odeme
            {
                IslemNo = iyzicoIslemNo,
                Tutar = gercekTutar,
                OdemeTarihi = DateTime.Now,
                OdemeDurumu = "Başarılı",
                SiparisID = yeniSiparis.ID
            };
            _context.Odemeler.Add(yeniOdeme);

            _context.SepetItems.RemoveRange(sepet.SepetItems);

            // Değişiklikleri veritabanına göndermeyi dene
            await _context.SaveChangesAsync();

            // --- 2. SİHİRLİ DOKUNUŞ: İŞLEMİ ONAYLA ---
            // Her şey sorunsuz çalıştıysa, tüm kayıtları kalıcı hale getir!
            await transaction.CommitAsync();

            return RedirectToAction("Basarili", "Siparis", new { siparisNo = yeniSiparis.SiparisNo });
        }
        catch (DbUpdateConcurrencyException)
        {
            // KORKULAN SENARYO GERÇEKLEŞTİ: Aynı anda iki kişi butona bastı!
            // Hemen bizim müşterimizin işlemini geri alıyoruz (Eksiye düşmeyi önlüyoruz).
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "Siz siparişi tamamlarken maalesef başka bir müşteri bu ürünün son stoğunu satın aldı. Lütfen sepetinizi güncelleyin.");
            return View(model);
        }
        catch (Exception ex)
        {
            // SUNUCU ÇÖKMESİ VEYA BEKLENMEYEN HATA DURUMU
            // Kimsenin parası boşa gitmesin, sipariş yarım kalmasın diye Rollback atıyoruz.
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "Siparişiniz oluşturulurken sistemsel bir hata meydana geldi. Lütfen tekrar deneyin.");
            // İleride buraya loglama ekleyebilirsin: _logger.LogError(ex, "Sipariş hatası");
            return View(model);
        }
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Siparislerim()
    {
        // 1. Giriş yapan kullanıcıyı bul
        string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
        {
            return RedirectToAction("Login", "Kullanici");
        }

        // 2. Kullanıcıya ait siparişleri tüm detaylarıyla (Ürünler, Durum vb.) getir
        // OrderByDescending ile en son verilen siparişi en üstte gösteriyoruz
        var siparisler = await _context.Siparisler
            .Include(s => s.SiparisDurumu) // Kargo durumu vs.
            .Include(s => s.SiparisDetaylari)
                .ThenInclude(sd => sd.Urun) // Siparişin içindeki ürünün resmi ve adı için
            .Where(s => s.KullaniciID == currentUserId)
            .OrderByDescending(s => s.SiparisTarihi)
            .ToListAsync();

        return View(siparisler);
    }
    public IActionResult Basarili(string siparisNo)
    {
        // Eğer sipariş numarası boş gelirse güvenliği sağla
        if (string.IsNullOrEmpty(siparisNo))
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.SiparisNo = siparisNo;
        return View();
    }
}