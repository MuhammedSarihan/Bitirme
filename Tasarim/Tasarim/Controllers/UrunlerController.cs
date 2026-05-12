using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.ExtensionMethods;
using Tasarim.Service.Abstract;
using Tasarim.Service.Concrete.LLM;
namespace Tasarim.Controllers
{
    public class UrunlerController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly KumelemeYoneticisi _kumelemeYoneticisi;
        // goruntu isleme icin eklendi
        private readonly IGeminiProvider _geminiProvider;
        public UrunlerController(DatabaseContext context,KumelemeYoneticisi kumelemeYoneticisi, IGeminiProvider geminiProvider)
        {
            _context = context;
            _kumelemeYoneticisi = kumelemeYoneticisi;
            _geminiProvider = geminiProvider; 
        }

        public async Task<IActionResult> Index(string q = "")
        {
            var query = _context.Urunler
                .Where(p => p.AktifMi)
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .Include(u => u.UrunOzellikleri) //  Teknik özellikleri de getir
                .Include(u => u.KampanyaUrunleri)
                    .ThenInclude(ku => ku.Kampanya)
                .AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                var kelimeler = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Her kelimeyi şart koşuyoruz (AND mantığı)
                foreach (var kelime in kelimeler)
                {
                    query = query.Where(p => p.Baslik.Contains(kelime) ||
                                             p.Aciklama.Contains(kelime) ||
                                             (p.UrunOzellikleri != null && (
                                                 p.UrunOzellikleri.Stil.Contains(kelime) ||
                                                 p.UrunOzellikleri.Detaylar.Contains(kelime)
                                             )));
                }
            }

            var urunlerListesi = await query.ToListAsync();
            if (!string.IsNullOrEmpty(q))
            {
                var kelimeler = q.ToLower().Split(' ');
                urunlerListesi = urunlerListesi
                    .OrderByDescending(u => kelimeler.Count(k => u.Baslik.ToLower().Contains(k)) +
                                            (u.UrunOzellikleri?.Stil.ToLower().Contains(kelimeler[0]) == true ? 2 : 0))
                    .ToList();
            }
            return View(urunlerListesi);
        }

        // URL: /Urunler/Details/1
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home"); // ID yoksa ana sayfaya at
            }

            // Ürünü ve içindeki Resim, Varyasyon (Beden), Kategori ve Marka listelerini çekiyoruz
            var urun = await _context.Urunler
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .Include(u => u.Resimler)
                .Include(u => u.Varyasyonlar)
                .Include(u => u.KampanyaUrunleri)
                    .ThenInclude(ku => ku.Kampanya)
                .Include(u => u.Yorumlar.Where(y => y.AnalizEdilirMi == 1 && y.YasakliKelime == false)) //Filtreli yorumlar
                    .ThenInclude(y => y.Profil)
                .Include(u => u.LLSonuc)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (urun == null || !urun.AktifMi)
            {
                // Ürün yoksa veya admin panelinden "Pasif" yapılmışsa müşteri göremez
                return RedirectToAction("Index", "Home");
            }
            //var aktifKampanyalar = urun.KampanyaUrunleri
            //    .Where(ku => ku.Kampanya != null && ku.Kampanya.KampanyaAktifMi)
            //    .Select(ku => ku.Kampanya);

            // 2. AYNI GRUP (FARKLI RENK) ÜRÜNLERİ BULMA
            // Eğer ürünün bir "Model Kodu" varsa (Örn: BEY3122), o koda sahip DİĞER ürünleri bul
            if (!string.IsNullOrEmpty(urun.ModelKodu))
            {
                var digerRenkler = await _context.Urunler
                    .Where(u => u.ModelKodu == urun.ModelKodu && u.ID != urun.ID && u.AktifMi)
                    .ToListAsync();

                // Sayfada göstermek için ViewBag ile gönderiyoruz
                ViewBag.AyniGrupUrunler = digerRenkler;
            }
            // --- ÜRÜN FAVORİLERDE EKLİ Mİ KONTROLÜ ---
            bool favorideMi = false;

            if (User.Identity!.IsAuthenticated)
            {
                // Giriş yapmışsa veritabanından bak
                int kullaniciId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
                favorideMi = _context.Favoriler.Any(f => f.UrunID == id && f.KullaniciID == kullaniciId);
            }
            else
            {
                // Ziyaretçiyse Session'dan bak
                var favorilerSession = HttpContext.Session.GetJson<List<Urun>>("GetFavoriler") ?? new List<Urun>();
                favorideMi = favorilerSession.Any(u => u.ID == id);
            }

            // Bu bilgiyi butonu değiştirmek için View'a gönderiyoruz
            ViewBag.FavorideMi = favorideMi;
            return View(urun);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YorumEkle(int UrunID, int Puan, string YorumIcerik)
        {
            // 1. Güvenlik: Kullanıcı giriş yapmamışsa logine at
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Hesap");
            }

            // 2. Giriş yapan kullanıcının ID'sini güvenli çerezden al
            int kullaniciId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);

            // 3. Bu kullanıcıya ait "Profil" kaydını bul (Senin servis ya da context yapına göre)
            var profil = await _context.Profiller.FirstOrDefaultAsync(p => p.KullaniciID == kullaniciId);

            if (profil != null)
            {
                // 4. Yeni yorumu oluştur ve Profile bağla
                var yeniYorum = new Yorum
                {
                    UrunID = UrunID,
                    ProfilID = profil.ID, // İşte kritik nokta burası!
                    Puan = Puan,
                    YorumIcerik = YorumIcerik,
                    Tarih = DateTime.Now
                };

                // 5. Veritabanına kaydet
                await _context.Set<Yorum>().AddAsync(yeniYorum);
                await _context.SaveChangesAsync();
            }

            // 6. Ürün detay sayfasına geri dön
            return RedirectToAction("Details", new { id = UrunID });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YorumSil(int YorumID, int UrunID)
        {
            // 1. Kullanıcı giriş yapmamışsa işlem yapma
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Hesap");
            }

            // 2. Giriş yapan kişinin ID'sini al
            int kullaniciId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);

            // 3. GÜVENLİK: Yorumu veritabanında bul, AMA sadece bu kullanıcıya aitse bul! 
            // (Başka biri tarayıcıdan ID değiştirip başkasının yorumunu silemesin diye)
            var silinecekYorum = await _context.Set<Yorum>()
                .Include(y => y.Profil)
                .FirstOrDefaultAsync(y => y.ID == YorumID && y.Profil.KullaniciID == kullaniciId);

            // 4. Eğer yorum bulunduysa ve bu kişiye aitse sil
            if (silinecekYorum != null)
            {
                _context.Set<Yorum>().Remove(silinecekYorum);
                await _context.SaveChangesAsync();

                await _kumelemeYoneticisi.UrunleriKumeleVeAnalizEtAsync(); //Ürün silindikten sonra LLSonuclarinin güncellenmesi için
            }

            // 5. Ürün detay sayfasına geri dön
            return RedirectToAction("Details", new { id = UrunID });
        }
        [HttpPost]
        public async Task<IActionResult> VisualSearch(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return Json(new { success = false, message = "Resim bulunamadı." });

            try
            {
                byte[] imageBytes;
                using (var ms = new MemoryStream())
                {
                    await imageFile.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                }

                // PROMPT
                string prompt = @"Sen profesyonel bir e-ticaret arama uzmanısın.
                Görevin: Görseldeki ürünü analiz etmek ve veritabanında en doğru eşleşmeyi bulmak için SADECE 2 veya 3 anahtar kelime döndürmektir.

                KRİTİK KURALLAR:
                1. FORMÜL: [Belirgin Renk] + [En Ayırıcı Teknik Özellik] + [Ürün Cinsi]
                2. AYIRICI ÖZELLİK ÖNCELİĞİ: 
                  - Pantolon ise paça tipi (İspanyol Paça, Skinny, Slouchy).
                  - Elbise ise kesim veya yaka tipi (Abiye, Mini, V Yaka, Şifon).
                  - Çanta ise askı veya doku tipi (Zincirli, El Çantası, Baget).
                  - Ayakkabı ise topuk veya burun tipi (Stiletto, Babet, Dolgu Topuk).
                3. YASAKLAR: Asla cümle kurma, açıklama yapma, 'Bu bir...' gibi ifadeler kullanma.
                4. SADECE arama kutusuna yazılacak net kelimeleri döndür. (Örn: 'Siyah İspanyol Paça Pantolon' veya 'Bej Zincirli Çanta')";

                var analizSonucu = await _geminiProvider.AnalyzeImageAsync(prompt, imageBytes);

                // Gereksiz boşlukları ve yapay zeka'nın bazen eklediği tırnakları temizle
                string temizSonuc = analizSonucu.Replace("\"", "").Trim();

                if (string.IsNullOrEmpty(temizSonuc) || temizSonuc.StartsWith("Hata"))
                {
                    return Json(new { success = false, message = "Ürün analiz edilemedi." });
                }

                var redirectUrl = Url.Action("Index", "Urunler", new { q = temizSonuc });
                return Json(new { redirectUrl = redirectUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
