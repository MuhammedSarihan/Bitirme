using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Tasarim.Service.Abstract; // IGeminiProvider yerine IVisionProvider gibi ortak bir interface kullanmanız daha iyi olabilir

namespace Tasarim.Service.Concrate.LLM;

public class GoruntuYerelProvider : IGoruntuProvider // Eğer arayüzü henüz ayırmadıysanız mevcut olanı kullanabilirsiniz
{
    private readonly HttpClient _http;
    private readonly string _ollamaUrl;

    public GoruntuYerelProvider(HttpClient http, IConfiguration config)
    {
        _http = http;

        var baseUrl = config["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _ollamaUrl = $"{baseUrl}/api/generate";
    }

    public async Task<string> AnalyzeImageAsync(string prompt, byte[] imageBytes)
    {
        var base64Image = Convert.ToBase64String(imageBytes);

        // Ollama'nın beklediği JSON formatı
        var requestBody = new
        {
            model = "gemma4:e2b", //modelimiz
            prompt = prompt,
            stream = false,
            images = new[] { base64Image } // Ollama görselleri bu dizide bekler
        };

        var request = new HttpRequestMessage(HttpMethod.Post, _ollamaUrl)
        {
            Content = JsonContent.Create(requestBody)
        };

        try
        {
            var response = await _http.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseString);
                // Ollama'nın yanıt döndürdüğü özellik "response" property'sidir
                return doc.RootElement.GetProperty("response").GetString() ?? "Analiz başarısız; model boş döndü.";
            }

            return $"Ollama Hatası: {response.StatusCode} - {responseString}";
        }
        catch (Exception ex)
        {
            return $"Sistem Hatası: {ex.Message}";
        }
    }
}