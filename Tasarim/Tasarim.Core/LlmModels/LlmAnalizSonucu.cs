using System.Text.Json.Serialization;

//Burada LLM'den dönen JSON'u karşılayacak C# sınıfını tanımlıyoruz.
//JSON'daki alan adlarıyla eşleşmesi için JsonPropertyName özniteliğini kullanıyoruz.
public class LlmAnalizSonucu
{
    [JsonPropertyName("duygu")]
    public string Duygu { get; set; } = string.Empty;

    [JsonPropertyName("artilar")]
    public List<string> Artilar { get; set; } = new();

    [JsonPropertyName("eksiler")]
    public List<string> Eksiler { get; set; } = new();

    [JsonPropertyName("sikayetler")]
    public List<string> Sikayetler { get; set; } = new();

    [JsonPropertyName("oneriler")]
    public List<string> Oneriler { get; set; } = new();
}