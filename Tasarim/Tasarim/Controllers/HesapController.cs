using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; // SignInAsync şeması için gerekli
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasarim.Core.Entities;
using Tasarim.Models.Hesap;
using Tasarim.Service.Abstract;

namespace Tasarim.Controllers
{
    public class HesapController : Controller
    {
        private readonly IService<Kullanici> _service;
        private readonly IService<Profil> _profilService;

        public HesapController(IService<Kullanici> service, IService<Profil> profilService)
        {
            _service = service;
            _profilService = profilService;
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
                var hesap = await _service.GetAsync(x => x.KullaniciAd == model.KullaniciAd && x.Sifre == model.Sifre);

                if (hesap == null)
                {
                    ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
                    return View(model); // Hata varsa hemen sayfayı döndür
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
                var mevcutKullanici = await _service.GetAsync(x => x.KullaniciAd == model.KullaniciAd);
                if (mevcutKullanici != null)
                {
                    ModelState.AddModelError("KullaniciAd", "Bu kullanıcı adı zaten kullanılıyor. Lütfen başka bir tane seçin.");
                    return View(model);
                }

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
                        Sifre = model.Sifre,
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

            // ÇÖZÜM BURADA: Formdan "Kullanici" tablosu (şifre vb.) gelmeyeceği için 
            // sistemin bunu hata olarak görüp işlemi iptal etmesini engelliyoruz.
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
    }
}