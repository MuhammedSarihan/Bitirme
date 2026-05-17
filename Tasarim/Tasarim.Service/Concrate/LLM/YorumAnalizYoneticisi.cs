using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web; //Türkçe Alfabe için
using System.Text.Json;
using System.Text.Unicode; //Türkçe Alfabe için
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.Service.Abstract;

namespace Tasarim.Service.Concrate.LLM;

public class YorumAnalizYoneticisi
{
    private readonly DatabaseContext _context;
    private readonly IYorumProvider _llmProvider;

    //Türkçe karakterlerin işlenmesi ve büyük/küçük harf duyarlılığının esnetilmesi için JsonSerialize ayarları
    private static readonly JsonSerializerOptions _jsonAyarlari = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNameCaseInsensitive = true 
    };
    public YorumAnalizYoneticisi(DatabaseContext context, IYorumProvider llmProvider)
    {
        _context = context;
        _llmProvider = llmProvider;
    }

    public async Task BekleyenYorumlariAnalizEtAsync(CancellationToken ct = default)
    {
        //YorumAnalizleri tablosunda YorumID'si BULUNMAYAN yorumları çek
        var bekleyenYorumlar = await _context.Yorumlar
                    .Where(y => (y.AnalizEdilirMi == 0 || y.AnalizEdilirMi == 1 || y.AnalizEdilirMi == 3)
                             && !_context.YorumAnalizleri.Any(ya => ya.YorumID == y.ID))
                    .ToListAsync(ct);

        if (bekleyenYorumlar.Count == 0)
            return;

        // Prompt şablonu belleği yormamak için döngü dışında sabit tutulur.

        string promptSablonu = @"Sen bir e-ticaret müşteri yorumu analiz sistemisin. Görevin, verilen yorumu analiz ederek SADECE belirtilen formatta geçerli bir JSON döndürmektir. Başka hiçbir giriş cümlesi veya açıklama yazma.

KURALLAR:
1. TOKSİK İÇERİK KONTROLÜ: Eğer yorumda açıkça veya sansürlenmiş/yıldızlı (örn: s*keyim, a*k, a.k. vb.) küfürler, hakaretler veya 'rezil', 'iğrenç', 'berbat', 'çöp' gibi kelimeler tespit edersen, ""toksiklik"": true yap. Diğer metin alanlarını ""Nötr"", tüm liste alanlarını ise [] yapıp analizi bitir.2. ÇEVİRİ: Yorum hangi dilde yazılmış olursa olsun, JSON içindeki tüm veriler kesinlikle Türkçe olmalıdır.
3. DUYGU KISITLAMASI: ""duygu"" değeri SADECE şu üç kelimeden biri olabilir: ""Pozitif"", ""Negatif"", ""Nötr"".
4. BOŞ DEĞERLER: Yorumda karşılığı olmayan özellik, şikayet veya öneri kategorileri için sadece boş liste [] döndür.

ÇIKTI ŞABLONU:
{{
  ""toksiklik"": false,
  ""duygu"": ""Pozitif"",
  ""artilar"": [""iyi özellik 1""],
  ""eksiler"": [""kötü özellik 1""],
  ""sikayetler"": [],
  ""oneriler"": []
}}

ANALİZ EDİLECEK YORUM:
""{0}""";

        foreach (var yorum in bekleyenYorumlar)
        {
            bool adminManuelOnayladi = (yorum.AnalizEdilirMi == 1);
            try
            {
                // Şablonun içindeki {0} alanına o anki yorumu yerleştiriyoruz
                var prompt = string.Format(promptSablonu, yorum.YorumIcerik);

                await Task.Delay(2000, ct);

                var jsonYanit = await _llmProvider.AnalyzeAsync(prompt, ct);

                jsonYanit = jsonYanit.Replace("```json", "").Replace("```", "").Trim();

                int baslangic = jsonYanit.IndexOf('{');
                int bitis = jsonYanit.LastIndexOf('}');

                if (baslangic >= 0 && bitis > baslangic)
                {
                    jsonYanit = jsonYanit.Substring(baslangic, bitis - baslangic + 1);
                }
                else
                {
                    throw new FormatException("LLM geçerli bir JSON formatı döndürmedi. Gelen Yanıt: " + jsonYanit);
                }

                // LlmAnalizSonucu kullanarak JSON'ı C# nesnesine çeviriyoruz
                var analizVerisi = JsonSerializer.Deserialize<LlmAnalizSonucu>(jsonYanit, _jsonAyarlari);

                if (analizVerisi != null)
                {
                    if (analizVerisi.Toksik && !adminManuelOnayladi)
                    {
                        yorum.YasakliKelime = true;
                        yorum.AnalizEdilirMi = 2;  // Admin onaylamadıysa ve toksikse 2 yap
                    }
                    else
                    {
                        yorum.YasakliKelime = false;
                        yorum.AnalizEdilirMi = 1; // Yasaklı değilse VEYA admin onayladıysa 1 yap

                        var yeniAnaliz = new YorumAnaliz
                        {
                            YorumID = yorum.ID,
                            Duygu = analizVerisi.Duygu,
                            Artilar = JsonSerializer.Serialize(analizVerisi.Artilar ?? new List<string>(), _jsonAyarlari),
                            Eksiler = JsonSerializer.Serialize(analizVerisi.Eksiler ?? new List<string>(), _jsonAyarlari),
                            Sikayetler = JsonSerializer.Serialize(analizVerisi.Sikayetler ?? new List<string>(), _jsonAyarlari),
                            Oneriler = JsonSerializer.Serialize(analizVerisi.Oneriler ?? new List<string>(), _jsonAyarlari)
                        };

                        _context.YorumAnalizleri.Add(yeniAnaliz);
                    }
                }
            }
            catch (Exception ex)
            {
                yorum.AnalizEdilirMi = 3; //Kafan karıştıysa 3 olarak işaretle
                Console.WriteLine($"ID: {yorum.ID} - Hata Mesajı: {ex.Message}");
            }
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception dbEx)
        {
            Console.WriteLine("Veritabanı toplu kayıt hatası: " + dbEx.Message);
        }
    }
}



