using System;
using System.Collections.Generic;
using System.Text;

// Hem OpenRouter'da hem de yerelde AnalyzeImageAsync metoduna sahip olmak zorunda kuralını tanımlıyoruz.
// Bu, her iki sınıfın da aynı işlevselliği sağlamasını ve benzer bir şekilde kullanılmasını sağlar.
// Bu tür bir tasarım, kodun daha tutarlı ve bakımı kolay olmasına yardımcı olur.
// Ayrıca, bu kural sayesinde, her iki sınıfın da aynı arayüzü uygulaması gerektiği için, kodun daha modüler ve genişletilebilir olmasını sağlar.


namespace Tasarim.Service.Abstract
{
    public interface IGoruntuProvider
    {
        Task<string> AnalyzeImageAsync(string prompt, byte[] imageBytes);
    }
}
