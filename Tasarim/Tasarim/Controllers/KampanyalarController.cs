using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tasarim.Data;

namespace Tasarim.Controllers
{
    public class KampanyalarController : Controller
    {
        private readonly DatabaseContext _context;

        public KampanyalarController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Kullanici/Kampanyalar

        public async Task<IActionResult> Index()
        {
            return View(await _context.Kampanyalar.ToListAsync());
        }

        // GET: Kullanici/Kampanyalar/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kampanya = await _context.Kampanyalar
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kampanya == null)
            {
                return NotFound();
            }

            return View(kampanya);
        }
    }
}
