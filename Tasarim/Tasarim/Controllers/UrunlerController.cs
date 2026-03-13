using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.ExtensionMethods;
using System.Linq;

namespace Tasarim.Controllers
{
    public class UrunlerController : Controller
    {
        private readonly DatabaseContext _context;

        public UrunlerController(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string q = "")
        {
            var databaseContext = _context.Urunler.Where(p => p.AktifMi && p.Baslik.Contains(q) || p.Aciklama.Contains(q) || p.ModelKodu.Contains(q))
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .Include(u => u.KampanyaUrunleri)
                    .ThenInclude(ku => ku.Kampanya);
            var urunlerListesi = await databaseContext.ToListAsync();


            foreach (var urun in urunlerListesi)
            {
                var aktifKampanyalar = urun.KampanyaUrunleri
                    .Where(ku => ku.Kampanya != null && ku.Kampanya.KampanyaAktifMi)
                    .Select(ku => ku.Kampanya);
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

            // 1. Ürünü ve içindeki Resim, Varyasyon (Beden), Kategori ve Marka listelerini çekiyoruz
            var urun = await _context.Urunler
                .Include(u => u.Kategori)
                .Include(u => u.Marka)
                .Include(u => u.Resimler)
                .Include(u => u.Varyasyonlar)
                .Include(u => u.KampanyaUrunleri)
                    .ThenInclude(ku => ku.Kampanya)
                 .Include(u => u.Yorumlar)
                    .ThenInclude(y => y.Profil)
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
            }

            // 5. Ürün detay sayfasına geri dön
            return RedirectToAction("Details", new { id = UrunID });
        }

    }
}
