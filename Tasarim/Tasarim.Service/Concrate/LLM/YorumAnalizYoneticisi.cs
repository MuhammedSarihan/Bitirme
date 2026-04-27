using LlmService;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tasarim.Core.Entities;
using Tasarim.Data;
using System.Text.Encodings.Web; //Türkçe Alfabe için
using System.Text.Unicode; //Türkçe Alfabe için
public class YorumAnalizYoneticisi
{
    private readonly DatabaseContext _context;
    private readonly ILlmProvider _llmProvider;

    public YorumAnalizYoneticisi(DatabaseContext context, ILlmProvider llmProvider)
    {
        _context = context;
        _llmProvider = llmProvider;
    }

    public async Task BekleyenYorumlariAnalizEtAsync(CancellationToken ct = default)
    {
        //YorumAnalizleri tablosunda YorumID'si BULUNMAYAN yorumları çek
        var bekleyenYorumlar = await _context.Yorumlar
            .Where(y => !_context.YorumAnalizleri.Any(ya => ya.YorumID == y.ID))
            .ToListAsync(ct);

        if (!bekleyenYorumlar.Any())
            return;

        foreach (var yorum in bekleyenYorumlar)
        {
            //Prompt
            var prompt = $@"Profesyonel bir e-ticaret veri analizi asistanısınız. Size gönderilen müşteri yorumlarını analiz edin ve sonuçları SADECE aşağıdaki JSON formatında döndürün.

KURALLAR:
- Yorum hangi dilde olursa olsun, sonucu Türkçe olarak döndürün.
- ""duygu"" için SADECE şu seçeneklerden birini kullanın: ""Pozitif"", ""Negatif"" veya ""Nötr"". Asla ""Karışık"" veya başka bir kelime yazmayın.
- Yorumda belirli bir kategoriye ait bilgi yoksa, o alanı boş bırakın ( [] ).
- Asla fazladan bir açıklama veya giriş cümlesi yazmayın; yalnızca JSON nesnesini döndürün.

Analiz Formatı:
{{
""duygu"": ""Pozitif/ Negatif/ Nötr"",
""artilar"": [""ürünün iyi özellikleri""],
""eksiler"": [""ürünün kötü özellikleri""],
""sikayetler"": [""ürün hakkındaki şikayetler""],
""oneriler"": [""ürünün geliştirilmesi için satıcıya veya diğer müşterilere öneriler""]
}}

Analiz Edilecek Yorum:
""{yorum.YorumIcerik}""";

            try
            {
                // Çalıştırma isteğini göönder (Groq/Ollama hangisi enjekte edildiyse o çalışır)
                var jsonYanit = await _llmProvider.AnalyzeAsync(prompt, ct);
                int baslangic = jsonYanit.IndexOf('{');
                int bitis = jsonYanit.LastIndexOf('}');

                if (baslangic >= 0 && bitis >= baslangic)
                {
                    // Sadece JSON kısmını çekip alıyoruz
                    jsonYanit = jsonYanit.Substring(baslangic, bitis - baslangic + 1);
                }
                else
                {
                    // Eğer metinde hiç süslü parantez yoksa, model tamamen saçmalamış demektir.
                    throw new Exception("LLM geçerli bir JSON formatı döndürmedi. Gelen Yanıt: " + jsonYanit);
                }
                // ========================================

                var analizVerisi = JsonSerializer.Deserialize<LlmAnalizSonucu>(jsonYanit);

                if (analizVerisi != null)
                {
                    // TÜRKÇE KARAKTER İZNİ VEREN AYAR:
                    var jsonAyarlari = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    };

                    var yeniAnaliz = new YorumAnaliz
                    {
                        YorumID = yorum.ID,
                        Duygu = analizVerisi.Duygu,

                        // jsonAyarlari
                        Artilar = JsonSerializer.Serialize(analizVerisi.Artilar, jsonAyarlari),
                        Eksiler = JsonSerializer.Serialize(analizVerisi.Eksiler, jsonAyarlari),
                        Sikayetler = JsonSerializer.Serialize(analizVerisi.Sikayetler, jsonAyarlari),
                        Oneriler = JsonSerializer.Serialize(analizVerisi.Oneriler, jsonAyarlari)
                    };

                    _context.YorumAnalizleri.Add(yeniAnaliz);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorum {yorum.ID} analiz edilemedi: {ex.Message}");
            }
        }

        //Döngü bittiğinde tüm yeni analizleri tek seferde veritabanına kaydet
        await _context.SaveChangesAsync(ct);
    }
}