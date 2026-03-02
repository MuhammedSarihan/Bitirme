using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasarim.Data;

namespace Tasarim.Controllers
{
    public class KategorilerController : Controller
    {
        private readonly DatabaseContext _context;

        public KategorilerController(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> IndexAsync(int? id)
        {
            if (id == null) return NotFound();

            var kategori = await _context.Kategoriler
                .Include(k => k.Urunler)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (kategori == null) return NotFound();

            return View(kategori);
        }
    }
}
