using System;
using System.Collections.Generic;
using System.Text;


//Hem Groq'ta hem de yerelde AnalyzeAsync metoduna sahip olmak zorunda kuralını tanımlıyoruz.
//Bu, her iki sınıfın da aynı işlevselliği sağlamasını ve benzer bir şekilde kullanılmasını sağlar.
//Bu tür bir tasarım, kodun daha tutarlı ve bakımı kolay olmasına yardımcı olur.
//Ayrıca, bu kural sayesinde, her iki sınıfın da aynı arayüzü uygulaması gerektiği için, kodun daha modüler ve genişletilebilir olmasını sağlar.
namespace LlmService;

public interface ILlmProvider
{
    Task<string> AnalyzeAsync(string prompt, CancellationToken ct = default);
}

