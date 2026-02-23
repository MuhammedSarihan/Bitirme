using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Tasarim.Data;
using Tasarim.Models;
using Microsoft.EntityFrameworkCore;

namespace Tasarim.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _context;

        public HomeController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {
            // Sliderları SıraNo'ya göre küçükten büyüğe sıralayarak çekiyoruz (senkron olarak)

            var model = new HomePageViewModel()
            {
                sliderListesi = await _context.Sliders.OrderBy(s => s.SiraNo).ToListAsync(),
                Urunler = await _context.Urunler.ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
