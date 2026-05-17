using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Tasarim.Service.Abstract;


//Bu sınıfta Groq API'sine istek atıyoruz
namespace Tasarim.Service.Concrate.LLM;

public class YorumAPIProvider : IYorumProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public YorumAPIProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        // appsettings.json'dan API key'i oku
        _apiKey = config["Groq:ApiKey"]
            ?? throw new Exception("Groq:ApiKey appsettings.json'da tanımlı değil");
    }

    public async Task<string> AnalyzeAsync(string prompt, CancellationToken ct = default)
    {
        // Groq'a gönderilecek istek formatı
        var requestBody = new
        {
            model = "qwen/qwen3-32b", //groq modelimiz
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        // İsteği hazırla
        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = JsonContent.Create(requestBody);

        // Gönder
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode(); // Başarısız yanıt durumlarında hata fırlat HTTPClient fonksiyonu

        // Cevabı oku
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }
}