using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; // SignInAsync şeması için gerekli
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasarim.Core.Entities;
using Tasarim.Models;
using Tasarim.Models.Hesap;
using Tasarim.Service.Abstract;

namespace Tasarim.Controllers
{
    public class HesapController : Controller
    {
        private readonly IKullaniciService _kullaniciService;
        private readonly IService<Profil> _profilService;

        public HesapController(IService<Profil> profilService, IKullaniciService kullaniciService)
        {
            _profilService = profilService;
            _kullaniciService = kullaniciService;
        }
        public async Task<IActionResult> Index()
        {
            // Giriş yapmamışsa doğrudan Login sayfasına at
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("SignIn");
            }

            // Giriş yapan kullanıcının ID'sini Claim'den al
            int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Profil tablosundan bu kullanıcıya ait bilgileri çek
            var profil = await _profilService.GetAsync(p => p.Kullanici.ID == kullaniciId);

            if (profil == null)
            {
                // Eğer profil bulunamazsa (hata durumu) çıkış yaptır
                return RedirectToAction("SignOut");
            }

            return View(profil); // Modeli sayfaya gönder
        }
        public IActionResult SignIn()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hesap = await _kullaniciService.GetAsync(x => x.KullaniciAd == model.KullaniciAd);


                if (hesap == null || !BCrypt.Net.BCrypt.Verify(model.Sifre, hesap.Sifre))
                {
                    ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
                    return View(model);
                }

                if (hesap.AktifMi == false)
                {
                    ModelState.AddModelError("", "⚠️ Hesabınız sistem yöneticisi tarafından askıya alınmıştır. Lütfen bizimle iletişime geçiniz.");
                    return View(model);
                }
                string rolIsmi = (hesap.AdminMi == true) ? "Admin" : "Musteri";
                string returnUrl = (hesap.AdminMi == true) ? "/Admin" : "/Hesap/Index";

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, hesap.KullaniciAd),
                    new Claim(ClaimTypes.Role, rolIsmi),
                    new Claim(ClaimTypes.NameIdentifier, hesap.ID.ToString())
                };

                var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);

                
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);

                return Redirect(returnUrl);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(KayitViewModel model)
        {
            if (ModelState.IsValid)
            {
                // EKLEME 1: Kullanıcı adı daha önce alınmış mı kontrolü
                var mevcutKullanici = await _kullaniciService.GetAsync(x => x.KullaniciAd == model.KullaniciAd);
                var mevcutmail = await _profilService.GetAsync(x => x.Mail == model.Mail);
                if (mevcutKullanici != null)
                {
                    ModelState.AddModelError("KullaniciAd", "Bu kullanıcı adı zaten kullanılıyor. Lütfen başka bir tane seçin.");
                    return View(model);
                }
                if (mevcutmail != null)
                {
                    ModelState.AddModelError("Mail", "Bu mail zaten kullanılıyor. Lütfen başka bir tane seçin.");
                    return View(model);
                }
                string hashlendi = BCrypt.Net.BCrypt.HashPassword(model.Sifre);
                var profil = new Profil
                {
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    TelNo = model.Tel,
                    Adres = model.Adres,
                    Cinsiyet = model.Cinsiyet,
                    Boy = model.Boy,
                    Kilo = model.Kilo,
                    Yas = model.Yas,
                    Mail = model.Mail,
                    Kullanici = new Kullanici
                    {
                        KullaniciAd = model.KullaniciAd,
                        Sifre = hashlendi,
                        AdminMi = false,
                        AktifMi = true
                    }
                };

                await _profilService.AddAsync(profil);
                await _profilService.SaveChangesAsync();

                // OTOMATİK GİRİŞ İŞLEMİ
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, profil.Kullanici.KullaniciAd),
                    new Claim(ClaimTypes.Role, "Musteri"),
                    // SignIn ile birebir aynı standart Claim'i kullanıyoruz
                    new Claim(ClaimTypes.NameIdentifier, profil.Kullanici.ID.ToString())
                };

                var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);

                return RedirectToAction("Index", "Hesap");
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ProfilDuzenle()
        {
            // 1. Giriş kontrolü
            if (!User.Identity!.IsAuthenticated) return RedirectToAction("SignIn");

            // 2. Kullanıcıyı bul ve mevcut bilgilerini forma gönder
            int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var profil = await _profilService.GetAsync(p => p.Kullanici.ID == kullaniciId);

            if (profil == null) return RedirectToAction("SignOut");

            return View(profil);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilDuzenle(Profil guncelModel)
        {
            if (!User.Identity!.IsAuthenticated) return RedirectToAction("SignIn");

            int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var mevcutProfil = await _profilService.GetAsync(p => p.Kullanici.ID == kullaniciId);

            ModelState.Remove("Kullanici");

            if (mevcutProfil != null && ModelState.IsValid)
            {
                mevcutProfil.Ad = guncelModel.Ad;
                mevcutProfil.Soyad = guncelModel.Soyad;
                mevcutProfil.TelNo = guncelModel.TelNo;
                mevcutProfil.Mail = guncelModel.Mail;
                mevcutProfil.Adres = guncelModel.Adres;
                mevcutProfil.Boy = guncelModel.Boy;
                mevcutProfil.Kilo = guncelModel.Kilo;
                mevcutProfil.Yas = guncelModel.Yas;
                mevcutProfil.Cinsiyet = guncelModel.Cinsiyet;

                _profilService.Update(mevcutProfil);
                await _profilService.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(guncelModel);
        }
        public async Task<IActionResult> SignOutAsync()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("SignIn");
        }
        // Şifre değiştirme
        [Authorize]
        public IActionResult SifreDegistir()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SifreDegistir(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var sonuc = await _kullaniciService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            if (sonuc)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
                return RedirectToAction("Index", "Hesap");
            }

            ModelState.AddModelError("OldPassword", "Mevcut şifreniz hatalı.");
            return View(model);
        }
    }
}