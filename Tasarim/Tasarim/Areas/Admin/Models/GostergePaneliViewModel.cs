using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Tasarim.Areas.Admin.Models
{
    public class GostergePaneliViewModel
    {
        // Duygu Dağılımı Verileri
        public int ToplamPozitif { get; set; }
        public int ToplamNotr { get; set; }
        public int ToplamNegatif { get; set; }
        public int ToplamYorum { get; set; }
        public int PozitifYuzde { get; set; }
        public int NotrYuzde { get; set; }
        public int NegatifYuzde { get; set; }
        public int BekleyenYorumSayisi { get; set; }
        public int? SeciliUrunId { get; set; }
        public string? SeciliUrunAd { get; set; }
        public List<SelectListItem>? UrunlerListesi { get; set; }
        // En Çok Tekrar Eden Artılar ve Eksiler
        public List<string>? Top5Artilar { get; set; }
        public List<string>? Top5Eksiler { get; set; }
        public List<string> EnPozitifUrunAdlari { get; set; } = new List<string>();
        public List<int> EnPozitifUrunSkorlari { get; set; } = new List<int>();
        public List<string> EnNegatifUrunAdlari { get; set; } = new List<string>();
        public List<int> EnNegatifUrunSkorlari { get; set; } = new List<int>();
        public List<Tasarim.Core.Entities.Siparis>? SonSiparisler { get; set; }
    }
}