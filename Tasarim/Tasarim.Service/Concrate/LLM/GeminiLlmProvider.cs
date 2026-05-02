using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using Tasarim.Service.Abstract;

namespace LlmService
{
    public class GeminiLlmProvider : IGeminiProvider
    {
        private readonly string _apiKey;
        private readonly string _modelName = "gemini-1.5-flash"; //modelimiz

        public GeminiLlmProvider(IConfiguration config)
        {
            _apiKey = config["GeminiConfig:ApiKey"]
                ?? throw new Exception("Gemini:ApiKey bulunamadı!");
        }

        public async Task<string> AnalyzeImageAsync(string prompt, byte[] imageBytes)
        {
            var googleAI = new GoogleAI(_apiKey);
            var model = googleAI.GenerativeModel(_modelName);

            // Kütüphanenin beklediği tam nesne yapısı:
            var request = new GenerateContentRequest(prompt);

            // Resim verisini base64 olarak ekliyoruz
            request.Contents[0].Parts.Add(new Part
            {
                InlineData = new InlineData
                {
                    MimeType = "image/jpeg",
                    Data = Convert.ToBase64String(imageBytes)
                }
            });

            var response = await model.GenerateContent(request);
            return response.Text;
        }
    }
}