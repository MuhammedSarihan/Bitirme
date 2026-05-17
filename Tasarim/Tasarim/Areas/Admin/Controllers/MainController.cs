using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tasarim.Areas.Admin.Models;
using Tasarim.Data;
using Tasarim.Service.Concrate.LLM;

namespace Tasarim.Areas.Admin.Controllers
{
    
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
   
    public class MainController : Controller
    {
        private readonly YorumAnalizYoneticisi _analizYoneticisi;
        private readonly KumelemeYoneticisi _kumelemeYoneticisi;
        private readonly DatabaseContext _context;
        private readonly UrunGorselYoneticisi _urunGorselYoneticisi;
        public MainController(DatabaseContext context, YorumAnalizYoneticisi analizYoneticisi, KumelemeYoneticisi kumelemeYoneticisi, UrunGorselYoneticisi urunGorselYoneticisi)
        {
            _analizYoneticisi = analizYoneticisi;
            _kumelemeYoneticisi = kumelemeYoneticisi;
            _context = context;
            _urunGorselYoneticisi = urunGorselYoneticisi;
        }

        //Yapay Zeka işlemlerini sırayla tetikleyen bir aksiyon
        [HttpPost]
        public async Task<IActionResult> BekleyenleriAnalizEt(bool isLocal)
        {
            try
            {
                await _analizYoneticisi.BekleyenYorumlariAnalizEtAsync(isLocal);
                await _kumelemeYoneticisi.UrunleriKumeleVeAnalizEtAsync();
                return Json(new { success = true, message = "Analiz işlemi başarıyla tamamlandı!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TopluGorselAnalizEt(bool isLocal)
        {
            try
            {
                int adet = await _urunGorselYoneticisi.AnalizEdilmemisGorselleriTopluAnalizEtAsync(isLocal);
                return Json(new { success = true, message = $"{adet} ürün başarıyla analiz edildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }


        // Ana ekran gösterge paneli aksiyonu
        [HttpGet]
        public IActionResult Index(int? urunId)
        {
            var model = new GostergePaneliViewModel();
            model.SeciliUrunId = urunId;

            int bekleyenYorumSayisi = _context.Yorumlar
                .Count(y => y.AnalizEdilirMi == 0 && !_context.YorumAnalizleri.Any(ya => ya.YorumID == y.ID));

            int bekleyenGorselSayisi = _context.Urunler
                .Count(u => u.AktifMi && !_context.UrunOzellikleri.Any(uo => uo.UrunID == u.ID));

            model.BekleyenYorumSayisi = bekleyenYorumSayisi;
            model.BekleyenGorselSayisi = bekleyenGorselSayisi;

            // Dropdown için ürün listesi
            model.UrunlerListesi = _context.Urunler
                .Select(u => new SelectListItem
                {
                    Value = u.ID.ToString(),
                    Text = u.UrunKod
                }).ToList();

            // Ana ekranda ürün seçiliyor ya da seçilmiyor bu bir filtreleme işlemi
            if (urunId.HasValue)
            {
                model.SeciliUrunAd = _context.Urunler
                    .Where(u => u.ID == urunId)
                    .Select(u => u.UrunKod)
                    .FirstOrDefault();
            }

            // Duygu dağılımını hesaplama 
            var yorumAnalizSorgusu = _context.YorumAnalizleri.AsQueryable();


            //Filtreli
            if (urunId.HasValue)
            {
                // YorumAnalizleri tablosu Yorumlar tablosu üzerinden UrunID'ye bağlanıyor
                yorumAnalizSorgusu = yorumAnalizSorgusu.Where(y => y.Yorum.UrunID == urunId);
            }

            var duyguGruplari = yorumAnalizSorgusu
                .GroupBy(y => y.Duygu)
                .Select(g => new { Duygu = g.Key, Adet = g.Count() })
                .ToList();

            model.ToplamPozitif = duyguGruplari.FirstOrDefault(x => x.Duygu == "Pozitif")?.Adet ?? 0;
            model.ToplamNegatif = duyguGruplari.FirstOrDefault(x => x.Duygu == "Negatif")?.Adet ?? 0;
            model.ToplamNotr = duyguGruplari.FirstOrDefault(x => x.Duygu == "Nötr" || x.Duygu == "Notr")?.Adet ?? 0;

            model.ToplamYorum = model.ToplamPozitif + model.ToplamNegatif + model.ToplamNotr;

            if (model.ToplamYorum > 0)
            {
                model.PozitifYuzde = (int)Math.Round((double)model.ToplamPozitif / model.ToplamYorum * 100);
                model.NegatifYuzde = (int)Math.Round((double)model.ToplamNegatif / model.ToplamYorum * 100);
                model.NotrYuzde = (int)Math.Round((double)model.ToplamNotr / model.ToplamYorum * 100);
            }

            // Top 5 Artılar ve Eksiler için LLSonuclari tablosundan verileri çekme
            var llSonuclariSorgusu = _context.LLSonuclari.AsQueryable();

            //Filtreli
            if (urunId.HasValue)
            {
                llSonuclariSorgusu = llSonuclariSorgusu.Where(l => l.UrunID == urunId);
            }

            var llSonuclari = llSonuclariSorgusu.ToList();
            var tumArtilar = new List<string>();
            var tumEksiler = new List<string>();
            var tumOneriler = new List<string>(); 
            var tumSikayetler = new List<string>(); 

            // Türkçe JSON çevirileri için
            var jsonOptions = new JsonSerializerOptions { AllowTrailingCommas = true };

            foreach (var sonuc in llSonuclari)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(sonuc.TopArtilar) && sonuc.TopArtilar != "[]")
                    {
                        var artilar = JsonSerializer.Deserialize<List<string>>(sonuc.TopArtilar, jsonOptions);
                        if (artilar != null) tumArtilar.AddRange(artilar);
                    }

                    if (!string.IsNullOrWhiteSpace(sonuc.TopEksiler) && sonuc.TopEksiler != "[]")
                    {
                        var eksiler = JsonSerializer.Deserialize<List<string>>(sonuc.TopEksiler, jsonOptions);
                        if (eksiler != null) tumEksiler.AddRange(eksiler);
                    }
                    if (!string.IsNullOrWhiteSpace(sonuc.TopOneriler) && sonuc.TopOneriler != "[]")
                    {
                        var oneriler = JsonSerializer.Deserialize<List<string>>(sonuc.TopOneriler, jsonOptions);
                        if (oneriler != null) tumOneriler.AddRange(oneriler);
                    }

                    if (!string.IsNullOrWhiteSpace(sonuc.TopSikayetler) && sonuc.TopSikayetler != "[]")
                    {
                        var sikayetler = JsonSerializer.Deserialize<List<string>>(sonuc.TopSikayetler, jsonOptions);
                        if (sikayetler != null) tumSikayetler.AddRange(sikayetler);
                    }
                }
                catch (JsonException)
                {
                    continue; // Hatalı JSON varsa atla sayfayı patlatma
                }
            }

            model.Top5Artilar = tumArtilar.GroupBy(x => x)
                                          .OrderByDescending(g => g.Count())
                                          .Take(5)
                                          .Select(g => g.Key).ToList();

            model.Top5Eksiler = tumEksiler.GroupBy(x => x)
                                          .OrderByDescending(g => g.Count())
                                          .Take(5)
                                          .Select(g => g.Key).ToList();

            model.Top5Oneriler = tumOneriler.GroupBy(x => x)
                                           .OrderByDescending(g => g.Count())
                                           .Take(5)
                                           .Select(g => g.Key).ToList();

            model.Top5Sikayetler = tumSikayetler.GroupBy(x => x)
                                               .OrderByDescending(g => g.Count())
                                               .Take(5)
                                               .Select(g => g.Key).ToList();

            // EN POZİTİF VE EN NEGATİF DÖNÜŞ ALAN 5'ER ÜRÜN (Filtreye Dahil Değil)

            // En Pozitifler (Pozitif yorum sayıları en fazla olanlar)
            var enPozitifler = _context.YorumAnalizleri
                .Where(y => y.Duygu == "Pozitif") // Sadece pozitif yorumları al
                .GroupBy(y => y.Yorum.UrunID)     // Ürünlere göre grupla
                .Select(g => new { UrunID = g.Key, PozitifSayisi = g.Count() }) // Pozitif yorum sayısını hesapla
                .OrderByDescending(g => g.PozitifSayisi) // En çok pozitif yorum alana göre sırala (IDye göre)
                .Take(5)
                .Join(_context.Urunler,
                      ya => ya.UrunID,
                      u => u.ID,
                      (ya, u) => new { UrunAd = u.UrunKod, Skor = ya.PozitifSayisi })
                .ToList();

            model.EnPozitifUrunAdlari = enPozitifler.Select(x => x.UrunAd).ToList();
            model.EnPozitifUrunSkorlari = enPozitifler.Select(x => x.Skor).ToList();

            // En Negatifler (Negatif yorum sayıları en fazla olanlar)
            var enNegatifler = _context.YorumAnalizleri
                .Where(y => y.Duygu == "Negatif") // Sadece negatif yorumları al
                .GroupBy(y => y.Yorum.UrunID)     // Ürünlere göre grupla
                .Select(g => new { UrunID = g.Key, NegatifSayisi = g.Count() }) // Negatif yorum sayısını hesapla
                .OrderByDescending(g => g.NegatifSayisi) // En çok negatif yorum alana göre sırala (IDye göre)
                .Take(5)
                .Join(_context.Urunler,
                      ya => ya.UrunID,
                      u => u.ID,
                      (ya, u) => new { UrunAd = u.UrunKod, Skor = ya.NegatifSayisi })
                .ToList();

            model.EnNegatifUrunAdlari = enNegatifler.Select(x => x.UrunAd).ToList();
            model.EnNegatifUrunSkorlari = enNegatifler.Select(x => x.Skor).ToList();

            model.SonSiparisler = _context.Siparisler
        .Include(s => s.Kullanici)       // Kullanıcı ID göstermek için
        .Include(s => s.SiparisDurumu)   // Durum (Hazırlanıyor, Kargolandı vs.)
        .OrderByDescending(s => s.SiparisTarihi) // En yeni sipariş en üstte
        .Take(10) // Sadece son 10 siparişi getir
        .ToList();

            return View(model);
        }
    }
}