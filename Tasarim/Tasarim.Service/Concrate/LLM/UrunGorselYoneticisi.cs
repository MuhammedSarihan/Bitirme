using System.Text.Json;
using Tasarim.Core.Entities;
using Tasarim.Service.Abstract;

namespace LlmService
{
    public class UrunGorselYoneticisi
    {
        private readonly IGeminiProvider _geminiProvider;

        public UrunGorselYoneticisi(IGeminiProvider geminiProvider)
        {
            _geminiProvider = geminiProvider;
        }

        public async Task<UrunOzellikleri> GorseliAnalizEt(byte[] imageBytes, int urunId)
        {
            // PROMPT: Maksimum hassasiyet için optimize edildi.
            string prompt = @"Sen profesyonel bir e-ticaret veri giriş uzmanı ve moda analistisin. 
            Görevin: Sana gönderilen ürün görselini analiz etmek ve sistemimize uygun veriyi hazırlamak.

            KURALLAR:
            1. SADECE aşağıda belirtilen JSON formatında cevap ver.
            2. JSON dışında asla açıklama, 'İşte analiziniz', 'Tabii ki' gibi giriş cümleleri yazma.
            3. Eğer görselden bir özelliği anlayamazsan boş bırakma, en mantıklı tahmini yap.
            4. Renkleri ana renkler (Kırmızı, Siyah, Lacivert vb.) olarak belirt.

            JSON FORMATI:
            {
              ""AnaKategori"": ""Ürünün en üst kategorisi (Örn: Ayakkabı, Üst Giyim, Aksesuar)"",
              ""AnaRenk"": ""Ürünün baskın rengi"",
              ""Materyal"": ""Ürünün yapıldığı ana malzeme (Örn: Deri, Pamuk, Polyester, Metal)"",
              ""Stil"": ""Ürünün tarzı (Örn: Casual, Klasik, Spor, Şık)"",
              ""Detaylar"": ""Ürünü tanımlayan, SEO uyumlu, kısa ve etkileyici bir cümle.""
            }";

            // Provider üzerinden analizi başlatıyoruz
            string rawResponse = await _geminiProvider.AnalyzeImageAsync(prompt, imageBytes);

            // Markdown ve olası boşluk temizliği
            string cleanJson = rawResponse
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            try
            {
                var result = JsonSerializer.Deserialize<UrunOzellikleri>(cleanJson);

                if (result != null)
                {
                    result.UrunID = urunId;
                }
                return result;
            }
            catch (Exception)
            {
                // Hata durumunda sistemin çökmemesi için fallback (yedek) nesne
                return new UrunOzellikleri
                {
                    UrunID = urunId,
                    Detaylar = "Görsel analiz edilemedi veya format hatası oluştu."
                };
            }
        }
    }
}