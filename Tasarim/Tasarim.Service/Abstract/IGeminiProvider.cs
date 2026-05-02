using System;
using System.Collections.Generic;
using System.Text;
using Tasarim.Core.Entities;

namespace Tasarim.Service.Abstract
{
    public interface IGeminiProvider
    {
        // Sınıfın içinde AnalyzeImageAsync yazdığın için burayı da böyle yapıyoruz
        Task<string> AnalyzeImageAsync(string prompt, byte[] imageBytes);
    }
}
