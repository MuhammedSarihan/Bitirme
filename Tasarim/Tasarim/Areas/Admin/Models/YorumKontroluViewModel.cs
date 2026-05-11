using Tasarim.Core.Entities;

namespace Tasarim.Areas.Admin.Models
{
    public class YorumKontroluViewModel
    {
        // 1. Tablonun verileri (Bildirimler)
        public IEnumerable<İletisim> Mesajlar { get; set; }

        // 2. Tablonun verileri (Yasaklı Yorumlar)
        public IEnumerable<Yorum> YasakliYorumlar { get; set; }
    }
}
