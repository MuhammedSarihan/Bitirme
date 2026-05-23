using System.Text.Encodings.Web; // Türkçe Alfabe için
using System.Text.Json;
using System.Text.Unicode; // Türkçe Alfabe için
using Tasarim.Core.Entities;
using Tasarim.Data; // DatabaseContext için
using Tasarim.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Tasarim.Service.Concrate.LLM;

public class UrunGorselYoneticisi
{
    private readonly GoruntuYerelProvider _yerelProvider;
    private readonly GoruntuAPIProvider _apiProvider;
    private readonly DatabaseContext _context;

    // Yorum analizindeki gibi Türkçe karakterlerin işlenmesi ve esnek JSON ayarları
    private static readonly JsonSerializerOptions _jsonAyarlari = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true // LLM fazladan virgül koyarsa çökmemesi için
    };

    public UrunGorselYoneticisi(GoruntuYerelProvider yerelProvider, GoruntuAPIProvider apiProvider, DatabaseContext context)
    {
        _yerelProvider = yerelProvider;
        _apiProvider = apiProvider;
        _context = context;
    }

    public async Task<int> AnalizEdilmemisGorselleriTopluAnalizEtAsync(bool isLocal, CancellationToken ct = default)
    {
        // SADECE AnalizTablosunda HİÇ kaydı olmayanları getir
        var bekleyenler = await _context.Urunler
            .Where(u => !string.IsNullOrEmpty(u.AnaResim))
            .Where(u => !_context.Set<UrunOzellikleri>().Any(o => o.UrunID == u.ID))
            .ToListAsync(ct);

        if (bekleyenler.Count == 0)
            return 0;

        int basariliSayisi = 0;
        string wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        // Prompt şablonu belleği yormamak için döngü dışında sabit tutulur.
        string promptSablonu = @"Sen profesyonel bir e-ticaret arama uzmanısın. 
Görevin: Görseldeki ürünü analiz etmek ve veritabanında en doğru eşleşmeyi bulmak için SADECE 2 veya 3 anahtar kelime döndürmektir.

KRİTİK KURALLAR:
1. FORMÜL: [Belirgin Renk] + [En Ayırıcı Teknik Özellik/Stil/Model] + [Kategori Adı]
Örnek: 'Siyah Oversize Tişört', 'Kahverengi Stiletto Ayakkabı', 'Mavi Flare Jean'

2. KATEGORİ ADI KURALI (Sadece ve sadece bu kelimelerden birini seçebilirsin, asla başka kelime uydurma):
   - Ürün bir tişört ise kesinlikle 'Tişört' yaz.
   - Ürün kadın bluzu ise kesinlikle 'Bluz' yaz.
   - Yakalı/düğmeli gömlek ise kesinlikle 'Gömlek' yaz.
   - Kot pantolon ise kesinlikle 'Jean' yaz.
   - Kumaş/Kargo/Keten pantolon ise kesinlikle 'Pantolon' yaz.
   - Diğer net kelimeler: 'Elbise', 'Abiye', 'Şort', 'Etek', 'Çanta', 'Ayakkabı'.

3. AYIRICI ÖZELLİK / STİL ÖNCELİĞİ (ÇOK KRİTİK):
   - Ayakkabılarda topukluysa kesinlikle 'Stiletto', düzse 'Babet' veya 'Spor' veya 'Makosen' yaz.
   - Çantalarda kısa askılı omuz çantasıysa 'Baget', elde taşınansa 'Portföy', sırttaysa 'Sırt' yaz.
   - Giysilerde kesim detayı çok önemlidir: 'Oversize', 'Flare', 'İspanyol Paça', 'Drape', 'Poplin'.

YASAKLAR: Asla açıklama yapma, cümle kurma. Sadece yan yana kelimeleri dön.";

        IGoruntuProvider aktifProvider = isLocal ? _yerelProvider : _apiProvider;

        // Bütün işlemleri tıpkı yorum analizindeki gibi tek döngüde yapıyoruz
        foreach (var urun in bekleyenler)
        {
            try
            {
                string sadeceDosyaAdi = Path.GetFileName(urun.AnaResim).Trim();
                string yol1 = Path.Combine(wwwroot, "img", sadeceDosyaAdi);
                string yol2 = Path.Combine(wwwroot, urun.AnaResim.Replace("/", "\\").TrimStart('\\'));

                string gercekYol = File.Exists(yol1) ? yol1 : (File.Exists(yol2) ? yol2 : null);

                if (gercekYol == null)
                    continue;

                byte[] imageBytes = await File.ReadAllBytesAsync(gercekYol, ct);

                // RTX 3060'ın VRAM'ini ve yerel sunucuyu yormamak için her görsel arası es veriyoruz
                await Task.Delay(2000, ct);

                string rawResponse = await aktifProvider.AnalyzeImageAsync(promptSablonu, imageBytes);
                // API'den veya LLM'den donanımsal/sistemsel bir hata dönerse atla (veritabanını kirletme)
                if (rawResponse.StartsWith("Hata") || rawResponse.StartsWith("Sistem") || rawResponse.StartsWith("Ollama"))
                {
                    Console.WriteLine($"Urun ID {urun.ID} için Hata: {rawResponse}");
                    continue;
                }

                // Yorum analizindeki güçlü Markdown/JSON temizlik algoritması
                string jsonYanit = rawResponse.Replace("```json", "").Replace("```", "").Trim();

                int baslangic = jsonYanit.IndexOf('{');
                int bitis = jsonYanit.LastIndexOf('}');

                if (baslangic >= 0 && bitis > baslangic)
                {
                    jsonYanit = jsonYanit.Substring(baslangic, bitis - baslangic + 1);
                }
                else
                {
                    throw new FormatException("LLM geçerli bir JSON formatı döndürmedi. Gelen Yanıt: " + rawResponse);
                }

                // JSON'ı güvenli bir şekilde C# nesnesine çevir (Türkçe karakter kaybı olmadan)
                var analizVerisi = JsonSerializer.Deserialize<UrunOzellikleri>(jsonYanit, _jsonAyarlari);

                if (analizVerisi != null)
                {
                    analizVerisi.UrunID = urun.ID;

                    // Veriyi context'e ekle (henüz veritabanına gitmiyor, hafızada bekliyor)
                    _context.Set<UrunOzellikleri>().Add(analizVerisi);
                    basariliSayisi++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Urun ID {urun.ID} Görsel Analiz Hatası: {ex.Message}");
            }
        }

        // Döngü bittikten sonra hafızada biriken tüm ürün özellikleri TEK SEFERDE veritabanına kaydedilir
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception dbEx)
        {
            Console.WriteLine("Veritabanı toplu kayıt hatası: " + dbEx.Message);
        }

        return basariliSayisi;
    }
}