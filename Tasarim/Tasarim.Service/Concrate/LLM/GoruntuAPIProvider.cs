using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Tasarim.Service.Abstract;

namespace Tasarim.Service.Concrate.LLM;

public class GoruntuAPIProvider : IGoruntuProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _modelName;

    public GoruntuAPIProvider(HttpClient http, IConfiguration config)
    {
        _http = http;

        // appsettings içindeki "ApiKey" değerini okur
        _apiKey = config["GeminiConfig:ApiKey"]
            ?? throw new Exception("ApiKey appsettings.json içerisinde bulunamadı!");

        // appsettings'te yoksa varsayılan olarak ücretsiz Gemma 4 modelini kullanır
        _modelName = config["GeminiConfig:ModelName"] ?? "google/gemma-4-31b-it:free ";         //model isminin yaninda bosluk kalinca calisti
    }

    public async Task<string> AnalyzeImageAsync(string prompt, byte[] imageBytes)
    {
        // OpenRouter tüm modelleri bu endpoint üzerinden çalıştırır
        

         var url = "https://openrouter.ai/api/v1/chat/completions";
        var requestBody = new
        {
            model = _modelName,
            messages = new[]
            {
                new {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new {
                            type = "image_url",
                            image_url = new {
                                url = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}"
                            }
                        }
                    }
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        // OpenRouter için Bearer Token kullanımı zorunludur
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        // Projeni OpenRouter üzerinde tanımlayan başlıklar
        request.Headers.Add("X-Title", "Urun Analiz Projesi");

        request.Content = JsonContent.Create(requestBody);

        try
        {
            var response = await _http.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseString);

                // OpenAI/OpenRouter standart yanıt hiyerarşisi
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "Analiz başarısız; model boş döndü.";
            }

            // Hata kodunu ve detayını veritabanına yazabilmen için döndürür
            return $"OpenRouter Hatası: {response.StatusCode} - {responseString}";
        }
        catch (Exception ex)
        {
            return $"Sistem Hatası: {ex.Message}";
        }
    }
}
