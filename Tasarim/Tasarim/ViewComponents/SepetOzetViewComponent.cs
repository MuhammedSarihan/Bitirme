using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasarim.Service.Abstract;

namespace Tasarim.ViewComponents 
{
    public class SepetOzetViewComponent : ViewComponent
    {
        private readonly ISepetService _sepetService;

        public SepetOzetViewComponent(ISepetService sepetService)
        {
            _sepetService = sepetService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int toplamAdet = 0;

            // Kullanıcı giriş yapmışsa sepetine bak, yapmamışsa 0 gönder
            if (UserClaimsPrincipal != null && UserClaimsPrincipal.Identity!.IsAuthenticated)
            {
                var userIdClaim = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int kullaniciId))
                {
                    var sepet = await _sepetService.GetSepetByKullaniciIdAsync(kullaniciId);
                    if (sepet != null && sepet.SepetItems != null)
                    {
                        // Sepetteki toplam adet sayısını hesapla
                        toplamAdet = sepet.SepetItems.Sum(x => x.Adet);
                    }
                }
            }

            return View(toplamAdet);
        }
    }
}