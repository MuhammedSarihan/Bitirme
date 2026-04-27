using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


//Bu sınıfta Yerel Ollama'ya istek atıyoruz.
namespace LlmService;

public class OllamaLlmProvider : ILlmProvider
{
    private readonly HttpClient _http;

    public OllamaLlmProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        // Ollama'nın çalıştığı adres — default localhost
        var baseUrl = config["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _http.BaseAddress = new Uri(baseUrl);
    }

    public async Task<string> AnalyzeAsync(string prompt, CancellationToken ct = default)
    {
        var requestBody = new
        {
            model = "gemma3:4b", //modelimiz
            prompt = prompt, //promptumuz
            stream = false  // Tek seferde cevap al, parça parça değil
        };

        // Gönder
        var response = await _http.PostAsJsonAsync("/api/generate", requestBody, ct);
        response.EnsureSuccessStatusCode(); // Başarısız yanıt durumlarında hata fırlat HTTPClient fonksiyonu

        // Cevabı oku
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json.GetProperty("response").GetString()!;
    }
}