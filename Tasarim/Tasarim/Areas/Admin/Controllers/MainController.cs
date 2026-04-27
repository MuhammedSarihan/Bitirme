using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasarim.Service.Concrete.LLM;

namespace Tasarim.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class MainController : Controller
    {
        private readonly YorumAnalizYoneticisi _analizYoneticisi;
        private readonly KumelemeYoneticisi _kumelemeYoneticisi;    

        // Servisimizi Dependency Injection ile çağırıyoruz
        public MainController(YorumAnalizYoneticisi analizYoneticisi, KumelemeYoneticisi kumelemeYoneticisi)
        {
            _analizYoneticisi = analizYoneticisi;
            _kumelemeYoneticisi = kumelemeYoneticisi;
        }

        // Bekleyen yorumları analiz eden aksiyon metodu
        [HttpPost]
        public async Task<IActionResult> BekleyenleriAnalizEt()
        {
            try
            {
                // Pipelinedaki 1. işlem
                await _analizYoneticisi.BekleyenYorumlariAnalizEtAsync();

                // Pipelinedaki 2. işlem
                await _kumelemeYoneticisi.UrunleriKumeleVeAnalizEtAsync();

                TempData["Mesaj"] = "Bekleyen tüm yorumlar başarıyla analiz edildi!";

            }
            catch (Exception ex)
            {
                TempData["Hata"] = $"İşlem zincirinde bir hata oluştu: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
