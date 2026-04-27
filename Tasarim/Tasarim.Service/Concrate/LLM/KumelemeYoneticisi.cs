using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System.Linq;
using Tasarim.Core.Entities;
using Tasarim.Core.LLMmodel;
using Tasarim.Data;

namespace Tasarim.Service.Concrete.LLM
{
    public class KumelemeYoneticisi
    {
        private readonly DatabaseContext _context;

        public KumelemeYoneticisi(DatabaseContext context)
        {
            _context = context;
        }

        public async Task UrunleriKumeleVeAnalizEtAsync(CancellationToken ct = default)
        {
            var mevcutAnalizler = await _context.LLSonuclari
                .AsNoTracking()
                .ToDictionaryAsync(l => l.UrunID, l => l.ToplamYorum, ct);

            //YorumAnalizleri tablosundan güncel verileri çek
            var tumYorumGruplari = await _context.YorumAnalizleri
                .Include(ya => ya.Yorum)
                .GroupBy(ya => ya.Yorum.UrunID)
                .Select(g => new
                {
                    UrunID = g.Key,
                    GuncelYorumSayisi = g.Count(),
                    Artilar = g.Select(x => x.Artilar ?? "").ToList(),
                    Eksiler = g.Select(x => x.Eksiler ?? "").ToList()
                })
                .ToListAsync(ct);

            var islenecekVeriListesi = new List<YorumVerisi>();

            foreach (var grup in tumYorumGruplari)
            {
                bool hicAnalizEdilmemis = !mevcutAnalizler.ContainsKey(grup.UrunID);
                bool yeniYorumVarMi = !hicAnalizEdilmemis && grup.GuncelYorumSayisi != mevcutAnalizler[grup.UrunID];

                if (hicAnalizEdilmemis || yeniYorumVarMi)
                {
                    islenecekVeriListesi.Add(new YorumVerisi
                    {
                        UrunID = grup.UrunID,
                        BirlesikYorum = string.Join(" ", grup.Artilar) + " " + string.Join(" ", grup.Eksiler),
                        HamArtilar = string.Join("|", grup.Artilar),
                        HamEksiler = string.Join("|", grup.Eksiler),
                        ToplamYorumSayisi = grup.GuncelYorumSayisi
                    });
                }
            }

            if (!islenecekVeriListesi.Any()) return;

            //ML.NET İşlemleri (Dinamik Küme Sayısı)
            var mlContext = new MLContext(seed: 42);
            var veriView = mlContext.Data.LoadFromEnumerable(islenecekVeriListesi);

            int dinamikKumeSayisi = Math.Min(islenecekVeriListesi.Count, 3);

            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(YorumVerisi.BirlesikYorum))
                .Append(mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: dinamikKumeSayisi));

            var model = pipeline.Fit(veriView);
            var predEngine = mlContext.Model.CreatePredictionEngine<YorumVerisi, KumeTahmini>(model);

            // Veritabanını Güncelleme
            foreach (var veri in islenecekVeriListesi)
            {
                var prediction = predEngine.Predict(veri);

                string duygu = prediction.SelectedClusterId switch
                {
                    1 => "Pozitif",
                    2 => "Negatif",
                    3 => "Nötr", // 3 gelirse direkt Nötr yaz
                };

                var mevcutKayit = await _context.LLSonuclari.FirstOrDefaultAsync(l => l.UrunID == veri.UrunID, ct);

                if (mevcutKayit != null)
                {
                    mevcutKayit.DuyguDagilim = duygu;
                    mevcutKayit.ToplamYorum = veri.ToplamYorumSayisi;
                    mevcutKayit.TopArtilar = GetTop5Phrases(veri.HamArtilar);
                    mevcutKayit.TopEksiler = GetTop5Phrases(veri.HamEksiler);
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
                        TopArtilar = GetTop5Phrases(veri.HamArtilar),
                        TopEksiler = GetTop5Phrases(veri.HamEksiler),
                        SonGuncelleme = DateTime.Now
                    };
                    await _context.LLSonuclari.AddAsync(yeni, ct);
                }
            }

            await _context.SaveChangesAsync(ct);
        }
        private string GetTop5Phrases(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "-";

            var phrases = text.Split(new[] { '|', ',', '.' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(p => p.Trim())
                              .Where(p => p.Length > 3)
                              .GroupBy(p => p, StringComparer.OrdinalIgnoreCase)
                              .OrderByDescending(g => g.Count())
                              .Take(5)
                              .Select(g => g.Key);

            return phrases.Any() ? string.Join(", ", phrases) : "-";
        }
    }
}