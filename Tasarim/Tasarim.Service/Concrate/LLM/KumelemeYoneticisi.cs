using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Tasarim.Core.Entities;
using Tasarim.Core.LLMmodel;
using Tasarim.Data;

namespace Tasarim.Service.Concrate.LLM;

public class KumelemeYoneticisi
{
    private readonly DatabaseContext _context;

    // TÜRKÇE KARAKTER DESTEĞİ İÇİN JSON AYARLARI:
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public KumelemeYoneticisi(DatabaseContext context)
    {
        _context = context;
    }

    public async Task UrunleriKumeleVeAnalizEtAsync(CancellationToken ct = default)
    {
        var mevcutAnalizler = await _context.LLSonuclari
            .AsNoTracking()
            .ToDictionaryAsync(l => l.UrunID, l => l.ToplamYorum, ct);

        var hamAnalizler = await _context.YorumAnalizleri
                .Include(ya => ya.Yorum)
                .Select(ya => new
                {
                    UrunID = ya.Yorum.UrunID,
                    Artilar = ya.Artilar,
                    Eksiler = ya.Eksiler,
                    Sikayetler = ya.Sikayetler,
                    Oneriler = ya.Oneriler
                })
                .ToListAsync(ct);

        var tumYorumGruplari = hamAnalizler
    .GroupBy(ya => ya.UrunID)
    .Select(g => new
    {
        UrunID = g.Key,
        GuncelYorumSayisi = g.Count(),
        Artilar = g.Select(x => x.Artilar ?? "").ToList(),
        Eksiler = g.Select(x => x.Eksiler ?? "").ToList(),
        Sikayetler = g.Select(x => x.Sikayetler ?? "").ToList(),
        Oneriler = g.Select(x => x.Oneriler ?? "").ToList()
    })
    .ToList();

        var islenecekVeriListesi = new List<YorumVerisi>();
        var islenecekUrunIdleri = new List<int>();
        var hamVeriler = new Dictionary<int, (List<string> Artilar, List<string> Eksiler, List<string> Sikayetler, List<string> Oneriler)>();

        foreach (var grup in tumYorumGruplari)
        {
            bool hicAnalizEdilmemis = !mevcutAnalizler.ContainsKey(grup.UrunID);
            bool yeniYorumVarMi = !hicAnalizEdilmemis && grup.GuncelYorumSayisi != mevcutAnalizler[grup.UrunID];

            if (hicAnalizEdilmemis || yeniYorumVarMi)
            {
                islenecekVeriListesi.Add(new YorumVerisi
                {
                    UrunID = grup.UrunID,
                    BirlesikYorum = string.Join(" ", grup.Artilar) + " " + string.Join(" ", grup.Eksiler) + " " + 
                                    string.Join(" ", grup.Sikayetler) + " " + string.Join(" ", grup.Oneriler),
                    ToplamYorumSayisi = grup.GuncelYorumSayisi
                });

                islenecekUrunIdleri.Add(grup.UrunID);
                hamVeriler.Add(grup.UrunID, (grup.Artilar, grup.Eksiler, grup.Sikayetler, grup.Oneriler));
            }
        }

        if (!islenecekVeriListesi.Any()) return;
        var tumVeriListesi = tumYorumGruplari.Select(grup => new YorumVerisi
        {
            UrunID = grup.UrunID,
            BirlesikYorum = string.Join(" ", grup.Artilar) + " " + string.Join(" ", grup.Eksiler) + " " +
                string.Join(" ", grup.Sikayetler) + " " + string.Join(" ", grup.Oneriler),
            ToplamYorumSayisi = grup.GuncelYorumSayisi
        }).ToList();

        int dinamikKumeSayisi = Math.Max(1, Math.Min(tumVeriListesi.Count, 3));
        var mlContext = new MLContext(seed: 42);

        var egitimVeriView = mlContext.Data.LoadFromEnumerable(tumVeriListesi);
        var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(YorumVerisi.BirlesikYorum))
            .Append(mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: dinamikKumeSayisi));

        var model = pipeline.Fit(egitimVeriView);

        var tahminVeriView = mlContext.Data.LoadFromEnumerable(islenecekVeriListesi);
        var transformedData = model.Transform(tahminVeriView);
        var predictions = mlContext.Data.CreateEnumerable<KumeTahmini>(transformedData, reuseRowObject: false).ToList();

        var guncellenecekKayitlar = await _context.LLSonuclari
            .Where(l => islenecekUrunIdleri.Contains(l.UrunID))
            .ToDictionaryAsync(l => l.UrunID, ct);

        for (int i = 0; i < islenecekVeriListesi.Count; i++)
        {
            var veri = islenecekVeriListesi[i];
            var prediction = predictions[i];

            string duygu = prediction.SelectedClusterId switch
            {
                1 => "Pozitif",
                2 => "Negatif",
                _ => "Nötr",
            };

            var hamVeri = hamVeriler[veri.UrunID];
            string jsonTopArtilar = GetTop5Phrases(hamVeri.Artilar);
            string jsonTopEksiler = GetTop5Phrases(hamVeri.Eksiler);
            string jsonTopSikayetler = GetTop5Phrases(hamVeri.Sikayetler);
            string jsonTopOneriler = GetTop5Phrases(hamVeri.Oneriler);

            if (guncellenecekKayitlar.TryGetValue(veri.UrunID, out var mevcutKayit))
            {
                mevcutKayit.DuyguDagilim = duygu;
                mevcutKayit.ToplamYorum = veri.ToplamYorumSayisi;
                mevcutKayit.TopArtilar = jsonTopArtilar;
                mevcutKayit.TopEksiler = jsonTopEksiler;
                mevcutKayit.TopSikayetler = jsonTopSikayetler;
                mevcutKayit.TopOneriler = jsonTopOneriler;
                mevcutKayit.SonGuncelleme = DateTime.Now;
                _context.LLSonuclari.Update(mevcutKayit);
            }
            else
            {
                var yeni = new LLSonuc
                {
                    UrunID = veri.UrunID,
                    DuyguDagilim = duygu,
                    ToplamYorum = veri.ToplamYorumSayisi,
                    TopArtilar = jsonTopArtilar,
                    TopEksiler = jsonTopEksiler,
                    TopSikayetler = jsonTopSikayetler,
                    TopOneriler = jsonTopOneriler,
                    SonGuncelleme = DateTime.Now
                };
                await _context.LLSonuclari.AddAsync(yeni, ct);
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private string GetTop5Phrases(List<string> phrasesList)
    {
        if (phrasesList == null || !phrasesList.Any()) return "[]";

        // Türkçe karakter için CultureInfo tanımı (Örn: I-ı, İ-i uyumu)
        var turkishCulture = new System.Globalization.CultureInfo("tr-TR");

        var topPhrases = phrasesList
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Replace("[", "").Replace("]", "").Replace("\"", "").Trim())
            .SelectMany(p => p.Split(new[] { '|', ',', '.' }, StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .Where(p => p.Length > 3)
            .GroupBy(p => p, StringComparer.Create(turkishCulture, true)) //Tanrı Türk'ü korusun
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        // Tanımladığımız _jsonOptions'ı kullanarak çeviri yapıyoruz
        return topPhrases.Any() ? JsonSerializer.Serialize(topPhrases, _jsonOptions) : "[]";
    }
}