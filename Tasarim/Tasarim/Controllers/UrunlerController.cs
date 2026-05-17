using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.ExtensionMethods;
using Tasarim.Service.Abstract;
using Tasarim.Service.Concrate.LLM;
namespace Tasarim.Controllers
{
    public class UrunlerController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly KumelemeYoneticisi _kumelemeYoneticisi;
        // goruntu isleme icin eklendi
        private readonly IGoruntuProvider _geminiProvider;
        public UrunlerController(DatabaseContext context, KumelemeYoneticisi kumelemeYoneticisi, IGoruntuProvider geminiProvider)
        {
            _context = context;
            _kumelemeYoneticisi = kumelemeYoneticisi;
            _geminiProvider = geminiProvider;
        }

        public async Task<IActionResult> Index(string q = "")
        {
            var query = _context.Urunler
                .Where(p => p.AktifMi)
                .Include(u => u.Varyasyonlar)
                .Include(u => u.Yorumlar.Where(y => y.AnalizEdilirMi == 1 && y.YasakliKelime == false))
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .Include(u => u.UrunOzellikleri)
                .Include(u => u.KampanyaUrunleri)
                    .ThenInclude(ku => ku.Kampanya)
                .AsQueryable();

            Func<string, string> temizle = (text) =>
            {
                if (string.IsNullOrEmpty(text)) return "";
                return text.ToLower()
                           .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
                           .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c")
                           .Replace("i̇", "i").Replace(" ", "").Trim();
            };

            string bulunanArananKategori = ""; // Sıralamada kullanmak için dışarıda tanımlıyoruz

            if (!string.IsNullOrEmpty(q))
            {
                var kelimeler = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // --- 1. ADIM: DİNAMİK KATEGORİ TESPİTİ ---
                var sistemdekiKategoriler = new List<string> { "jean", "pantolon", "sort", "etek", "elbise", "abiye", "bluz", "gomlek", "tisort", "kazak", "canta", "ayakkabi", "kolye", "kupe", "bilezik", "taki", "ustgiyim" };

                bulunanArananKategori = kelimeler
                    .Select(k => k.ToLower().Replace("ı", "i").Replace("ç", "c").Replace("ş", "s").Replace(" ", ""))
                    .FirstOrDefault(k => sistemdekiKategoriler.Contains(k) || k == "kot");

                if (bulunanArananKategori == "kot") bulunanArananKategori = "jean";

                // --- 2. ADIM: KATEGORİ GÜVENLİK DUVARI (YENİLENDİ!) ---
                if (!string.IsNullOrEmpty(bulunanArananKategori))
                {
                    // EĞER ARANAN "JEAN" İSE: Sadece Jean veya Kot olanlar sızabilir. Kumaş pantolonlar elenir!
                    if (bulunanArananKategori == "jean")
                    {
                        query = query.Where(p => p.UrunOzellikleri != null &&
                                                (p.UrunOzellikleri.AnaKategori.ToLower().Contains("jean") ||
                                                 p.Baslik.ToLower().Contains("jean")));
                    }
                    // EĞER ARANAN KUMAŞ "PANTOLON" İSE: İçinde jean/kot geçen her şeyi bıçak gibi keser!
                    else if (bulunanArananKategori == "pantolon")
                    {
                        query = query.Where(p => p.UrunOzellikleri != null &&
                                                p.UrunOzellikleri.AnaKategori.ToLower().Contains("pantolon") &&
                                                !p.UrunOzellikleri.AnaKategori.ToLower().Contains("jean") &&
                                                !p.Baslik.ToLower().Contains("jean"));
                    }
                    // EĞER ARANAN "GOMLEK" VEYA "BLUZ" İSE: Birbirlerinin alanlarına sızmalarını engeller!
                    else if (bulunanArananKategori == "gomlek" || bulunanArananKategori == "bluz")
                    {
                        query = query.Where(p => p.UrunOzellikleri != null &&
                                                (p.UrunOzellikleri.AnaKategori.ToLower().Contains(bulunanArananKategori) ||
                                                 p.Baslik.ToLower().Contains(bulunanArananKategori)));
                    }
                    // DİĞER EVRENSEL KATEGORİLER (Elbise, Abiye, Çanta vb.)
                    else
                    {
                        query = query.Where(p => p.UrunOzellikleri != null &&
                                                (p.UrunOzellikleri.AnaKategori.ToLower().Contains(bulunanArananKategori) ||
                                                 p.Baslik.ToLower().Contains(bulunanArananKategori)));
                    }
                }

                // --- 3. ADIM: KATEGORİ İÇİNDE ESNEK OR ARAMASI ---
                query = query.Where(p => kelimeler.Any(kelime =>
                    p.Baslik.ToLower().Contains(kelime.ToLower()) ||
                    p.Aciklama.ToLower().Contains(kelime.ToLower()) ||
                    (p.UrunOzellikleri != null && (
                        p.UrunOzellikleri.AnaRenk.ToLower().Contains(kelime.ToLower()) ||
                        p.UrunOzellikleri.Stil.ToLower().Contains(kelime.ToLower()) ||
                        p.UrunOzellikleri.Detaylar.ToLower().Contains(kelime.ToLower())
                    ))
                ));
            }

            var urunlerListesi = await query.ToListAsync();

            if (!string.IsNullOrEmpty(q))
            {
                var kelimeler = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // --- 4. ADIM: AKILLI PUANLAMA VE SIRALAMA  ---
                urunlerListesi = urunlerListesi
                    .OrderByDescending(u =>
                    {
                        int puan = 0;
                        string baslik = temizle(u.Baslik);
                        string kategori = temizle(u.UrunOzellikleri?.AnaKategori);
                        string renk = temizle(u.UrunOzellikleri?.AnaRenk);
                        string stil = temizle(u.UrunOzellikleri?.Stil);
                        string detaylar = temizle(u.UrunOzellikleri?.Detaylar);

                        // --- ALTIN KURAL: KATEGORİ TAMAMEN TUTUYORSA DEV BONUS PUAN (+5000 PUAN) ---
                        // Böylece aranan kategoriyle %100 uyuşan ürünler askeri nizamda en üst sıraya dizilir!
                        if (!string.IsNullOrEmpty(bulunanArananKategori))
                        {
                            bool kategoriTamEslesti = kategori.Contains(bulunanArananKategori) || baslik.Contains(bulunanArananKategori);
                            if (kategoriTamEslesti) puan += 5000;
                        }

                        foreach (var k in kelimeler)
                        {
                            string temizKelime = temizle(k);

                            // 1. Öncelik: Belirgin Stil / Teknik Özellik (Drape, Halter yaka, İspanyol paça) -> +100 Puan
                            if (stil.Contains(temizKelime) || detaylar.Contains(temizKelime) || temizKelime.Contains(stil) || temizKelime.Contains(detaylar))
                                puan += 100;

                            // 2. Öncelik: Renk Birebir Eşleşiyorsa -> +50 Puan
                            if (renk.Contains(temizKelime) || baslik.Contains(temizKelime))
                                puan += 50;

                            // 3. Öncelik: Genel Başlık Kelimeleri -> +20 Puan
                            if (baslik.Contains(temizKelime))
                                puan += 20;
                        }
                        return puan;
                    })
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
1. FORMÜL: [Belirgin Renk] + [En Ayırıcı Teknik Özellik] + [Kategori Adı]

2. KATEGORİ ADI KURALI (Sadece ve sadece bu kelimelerden birini seçebilirsin, asla başka kelime uydurma):
   - Ürün bir kadın bluzu ise kesinlikle 'Bluz' yaz.
   - Ürün yakalı/düğmeli gömlek ise kesinlikle 'Gömlek' yaz.
   - Kot pantolon ise kesinlikle 'Jean' yaz.
   - Kumaş/Kargo/Keten pantolon ise kesinlikle 'Pantolon' yaz.
   - Diğer net kelimeler: 'Elbise', 'Abiye', 'Şort', 'Etek', 'Çanta', 'Ayakkabı', 'Takı'.

3. AYIRICI ÖZELLİK ÖNCELİĞİ:
   - Kesim, yaka veya doku detayını yakala (Örn: 'Taşlı', 'Drape', 'Halter Yaka', 'İspanyol Paça', 'Simli', 'Düşük Omuz').

YASAKLAR: Asla açıklama yapma, cümle kurma. Sadece arama kutusuna yazılacak kelimeleri dön.
Örn: 'Siyah Taşlı Abiye' veya 'Mavi İspanyol Paça Jean' veya 'Kahverengi Drape Bluz'";

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
