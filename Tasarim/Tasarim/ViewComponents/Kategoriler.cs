using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tasarim.Data;

namespace Tasarim.ViewComponents
{
    public class Kategoriler : ViewComponent
    {
        private readonly DatabaseContext _context;

        public Kategoriler(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await _context.Kategoriler.ToListAsync());
        }


    }
}
