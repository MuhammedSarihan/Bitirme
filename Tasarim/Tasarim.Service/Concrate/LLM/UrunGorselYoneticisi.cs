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
            string prompt = @"Sen teknik bir veri ayıklama robotusun. 
           Görevin: Görseli analiz et ve verileri SADECE JSON objesi olarak döndür.
           KRİTİK KURALLAR:
           1. JSON dışında tek bir kelime bile yazma.
           2. Kod blokları (```json ) kullanma, doğrudan '{' ile başla ve '}' ile bitir.
           3. Değerler boş olamaz; 'Bilinmiyor' veya en yakın tahmini yaz.

          JSON FORMATI:
          {
          ""AnaKategori"": ""Kategori adı"",
          ""AnaRenk"": ""Baskın renk"",
          ""Materyal"": ""Malzeme tipi"",
          ""Stil"": ""Tarz örneği"",
          ""Detaylar"": ""SEO uyumlu kısa açıklama""
          }";

            string rawResponse = await _geminiProvider.AnalyzeImageAsync(prompt, imageBytes);

            // EĞER GELEN YANIT "Hata" İLE BAŞLIYORSA JSON OLARAK İŞLEME, DOĞRUDAN KAYDET
            if (rawResponse.StartsWith("Hata"))
            {
                return new UrunOzellikleri
                {
                    UrunID = urunId,
                    Detaylar = rawResponse // Hatayı buraya yazdırıyoruz
                };
            }


            // Daha agresif temizlik: Sadece { ve } arasını al
            string cleanJson = rawResponse;
            int start = cleanJson.IndexOf("{");
            int end = cleanJson.LastIndexOf("}");
            if (start != -1 && end != -1)
            {
                cleanJson = cleanJson.Substring(start, (end - start) + 1);
            }
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var result = JsonSerializer.Deserialize<UrunOzellikleri>(cleanJson, options);

                if (result != null)
                {
                    result.UrunID = urunId;
                    if (string.IsNullOrEmpty(result.AnaKategori))
                    {
                        result.Detaylar = "Ham Yanit: " + cleanJson.Substring(0, Math.Min(cleanJson.Length, 100));
                    }
                }
                return result;
            } // Try burada bitiyor
            catch (Exception ex)
            {
                // Hata durumunda burası çalışır
                return new UrunOzellikleri
                {
                    UrunID = urunId,
                    Detaylar = "Hata oluştu: " + ex.Message
                };
            }
        }


        }
    }















